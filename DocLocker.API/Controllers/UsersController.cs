using DocLocker.API.Services;
using DocLocker.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocLocker.API.Controllers
{
    // User management.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        // Initialize with user service and logger.
        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // This reads the current admin context from JWT claims for authorization checks.
        private bool TryGetAdminContext(out int currentUserId, out bool currentUserIsSuperAdmin, out IActionResult? errorResult)
        {
            currentUserId = 0;
            currentUserIsSuperAdmin = false;
            errorResult = null;

            var allowClaim = User.FindFirst("AllowUserManagement")?.Value;
            if (!bool.TryParse(allowClaim, out var allowUserManagement) || !allowUserManagement)
            {
                _logger.LogWarning("Admin user management access denied due to AllowUserManagement claim.");
                errorResult = Forbid();
                return false;
            }

            var superAdminClaim = User.FindFirst("IsSuperAdmin")?.Value;
            currentUserIsSuperAdmin = bool.TryParse(superAdminClaim, out var isSuperAdmin) && isSuperAdmin;

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out currentUserId))
            {
                _logger.LogWarning("Admin user management access denied due to missing user id claim.");
                errorResult = Unauthorized();
                return false;
            }

            return true;
        }

        // Create a new user.
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDTO dto)
        {
            _logger.LogInformation("Admin user creation API called. Email: {Email}, RoleId: {RoleId}", dto.Email, dto.RoleId);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin user creation failed due to invalid model state. Email: {Email}", dto.Email);
                return BadRequest(ModelState);
            }

            if (!TryGetAdminContext(out var currentUserId, out var currentUserIsSuperAdmin, out var errorResult))
            {
                return errorResult!;
            }

            var result = await _userService.CreateUserAsync(dto, currentUserId, currentUserIsSuperAdmin);
            if (!result.Success)
            {
                _logger.LogWarning("Admin user creation failed. Email: {Email}, Error: {Error}", dto.Email, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            _logger.LogInformation("Admin user creation succeeded. UserId: {UserId}, Email: {Email}", result.UserId, dto.Email);
            return Ok(new { UserId = result.UserId });
        }

        // Return all users.
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Admin user list API called.");
            if (!TryGetAdminContext(out _, out _, out var errorResult))
            {
                return errorResult!;
            }
            var users = await _userService.GetAllAsync();
            _logger.LogInformation("Admin user list retrieval succeeded. Count: {Count}", users.Count);
            return Ok(users);
        }

        // Update an existing user.
        [HttpPut("{userId:int}")]
        public async Task<IActionResult> UpdateUser(int userId, UpdateUserDTO dto)
        {
            _logger.LogInformation("Admin user update API called. UserId: {UserId}, RoleId: {RoleId}", userId, dto.RoleId);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin user update failed due to invalid model state. UserId: {UserId}", userId);
                return BadRequest(ModelState);
            }

            if (!TryGetAdminContext(out var currentUserId, out var currentUserIsSuperAdmin, out var errorResult))
            {
                return errorResult!;
            }

            var result = await _userService.UpdateAsync(userId, dto, currentUserId, currentUserIsSuperAdmin);
            if (!result.Success)
            {
                _logger.LogWarning("Admin user update failed. UserId: {UserId}, Error: {Error}", userId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            _logger.LogInformation("Admin user update succeeded. UserId: {UserId}", userId);
            return Ok();
        }

        // Toggle user activation.
        [HttpPatch("{userId:int}/activation")]
        public async Task<IActionResult> ToggleActivation(int userId)
        {
            _logger.LogInformation("Admin user activation toggle API called. UserId: {UserId}", userId);
            if (!TryGetAdminContext(out var currentUserId, out var currentUserIsSuperAdmin, out var errorResult))
            {
                return errorResult!;
            }

            var result = await _userService.ToggleActiveAsync(userId, currentUserId, currentUserIsSuperAdmin);
            if (!result.Success)
            {
                _logger.LogWarning("Admin user activation toggle failed. UserId: {UserId}, Error: {Error}", userId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            _logger.LogInformation("Admin user activation toggle succeeded. UserId: {UserId}, IsActive: {IsActive}", userId, result.IsActive);
            return Ok(new { result.IsActive });
        }
    }
}
