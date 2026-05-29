using Microsoft.AspNetCore.Mvc;
using DocLocker.API.Data;
using DocLocker.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace DocLocker.API.Controllers
{
    // Handles authentication operations like registration and login.
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DocLockerDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        // Dependencies for authS.
        public AuthController(DocLockerDbContext context, IConfiguration config, ILogger<AuthController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        // Register 
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            _logger.LogInformation("Registration API called. Email: {Email}", dto.Email);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration failed due to invalid model state. Email: {Email}", dto.Email);
                return BadRequest(ModelState);
            }

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                _logger.LogWarning("Registration attempt with duplicate email: {Email}", dto.Email);
                return BadRequest("Email already exists");
            }

            var memberRoleId = 3;
            if (!await _context.Roles.AnyAsync(r => r.RoleId == memberRoleId))
            {
                _logger.LogWarning("Registration failed due to missing member role.");
                return BadRequest("Member role is not configured");
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                RoleId = memberRoleId,
                AllowUserManagement = false,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully. UserId: {UserId}, Email: {Email}", user.UserId, dto.Email);
            return Ok("User registered successfully");
        }

        // Log 
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            _logger.LogInformation("Login API called. Email: {Email}", dto.Email);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login failed due to invalid model state. Email: {Email}", dto.Email);
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed due to invalid credentials. Email: {Email}", dto.Email);
                return Unauthorized("Invalid credentials");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for deactivated user: {Email}", user.Email);
                return Unauthorized("User is deactivated");
            }

            var token = GenerateJwtToken(user);

            _logger.LogInformation("Login succeeded. UserId: {UserId}, Email: {Email}", user.UserId, user.Email);

            return Ok(new AuthResponseDTO
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name,
                AllowUserManagement = user.AllowUserManagement,
                IsSuperAdmin = user.IsSuperAdmin
            });
        }

        // Create the JWT token.
        private string GenerateJwtToken(User user)
        {
            var roleName = user.Role?.Name ?? string.Empty;
            // This adds the user management flags to the token for authorization checks.
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("AllowUserManagement", user.AllowUserManagement.ToString()),
                new Claim("IsSuperAdmin", user.IsSuperAdmin.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.AddHours(1),
                claims: claims,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
