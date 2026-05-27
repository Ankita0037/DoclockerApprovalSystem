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

        // Create a new user.
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _userService.CreateUserAsync(dto);
                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(new { UserId = result.UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin user creation failed.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user");
            }
        }

        // Return all users.
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin user list retrieval failed.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching users");
            }
        }

        // Update an existing user.
        [HttpPut("{userId:int}")]
        public async Task<IActionResult> UpdateUser(int userId, UpdateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _userService.UpdateAsync(userId, dto);
                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin user update failed for user id: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the user");
            }
        }

        // Toggle user activation.
        [HttpPatch("{userId:int}/activation")]
        public async Task<IActionResult> ToggleActivation(int userId)
        {
            try
            {
                var result = await _userService.ToggleActiveAsync(userId);
                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(new { result.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin user activation toggle failed for user id: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating user activation");
            }
        }
    }
}
