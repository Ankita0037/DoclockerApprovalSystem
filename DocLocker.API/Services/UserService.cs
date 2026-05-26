using DocLocker.API.Data;
using DocLocker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocLocker.API.Services
{
    public class UserService : IUserService
    {
        private readonly DocLockerDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(DocLockerDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string? ErrorMessage, int? UserId)> CreateUserAsync(CreateUserDTO dto)
        {
            try
            {
                _logger.LogInformation("Admin user creation started for email: {Email}", dto.Email);

                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                {
                    _logger.LogWarning("Admin user creation failed due to duplicate email: {Email}", dto.Email);
                    return (false, "Email already exists", null);
                }

                if (dto.RoleId != 2 && dto.RoleId != 3)
                {
                    _logger.LogWarning("Admin user creation failed due to invalid role: {RoleId}", dto.RoleId);
                    return (false, "Role must be Manager or Member", null);
                }

                var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == dto.RoleId);
                if (!roleExists)
                {
                    _logger.LogWarning("Admin user creation failed because role not configured: {RoleId}", dto.RoleId);
                    return (false, "Role is not configured", null);
                }

                var user = new User
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    RoleId = dto.RoleId,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin user created successfully: {Email}", dto.Email);
                return (true, null, user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin user creation error for email: {Email}", dto.Email);
                throw;
            }
        }

        public async Task<IReadOnlyList<UserSummaryDTO>> GetAllAsync()
        {
            _logger.LogInformation("Admin user list retrieval started.");

            var users = await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Select(u => new UserSummaryDTO
                {
                    UserId = u.UserId,
                    Name = u.FullName,
                    Email = u.Email,
                    RoleName = u.Role != null ? u.Role.Name : string.Empty,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            _logger.LogInformation("Admin user list retrieval completed. Count: {Count}", users.Count);
            return users;
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int userId, UpdateUserDTO dto)
        {
            try
            {
                _logger.LogInformation("Admin user update started for user id: {UserId}", userId);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    _logger.LogWarning("Admin user update failed. User not found: {UserId}", userId);
                    return (false, "User not found");
                }

                if (dto.RoleId != 2 && dto.RoleId != 3)
                {
                    _logger.LogWarning("Admin user update failed due to invalid role: {RoleId}", dto.RoleId);
                    return (false, "Role must be Manager or Member");
                }

                var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == dto.RoleId);
                if (!roleExists)
                {
                    _logger.LogWarning("Admin user update failed because role not configured: {RoleId}", dto.RoleId);
                    return (false, "Role is not configured");
                }

                user.FullName = dto.FullName;
                user.PhoneNumber = dto.PhoneNumber;
                user.RoleId = dto.RoleId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin user updated successfully. User id: {UserId}", userId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin user update error for user id: {UserId}", userId);
                throw;
            }
        }

        public async Task<(bool Success, string? ErrorMessage, bool? IsActive)> ToggleActiveAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Admin user activation toggle started for user id: {UserId}", userId);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    _logger.LogWarning("Admin user activation toggle failed. User not found: {UserId}", userId);
                    return (false, "User not found", null);
                }

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin user activation toggled. User id: {UserId}, IsActive: {IsActive}", userId, user.IsActive);
                return (true, null, user.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin user activation toggle error for user id: {UserId}", userId);
                throw;
            }
        }
    }
}
