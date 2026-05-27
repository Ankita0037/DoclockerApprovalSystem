using DocLocker.API.Services;
using DocLocker.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocLocker.API.Controllers
{
    // Project management.
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectsController> _logger;

        // Initialize with project service and logger.
        public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        // Create a new project as admin.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProject(CreateProjectDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!int.TryParse(User.FindFirstValue("UserId"), out var adminId))
            {
                _logger.LogWarning("Admin project creation failed due to missing admin id claim.");
                return Unauthorized();
            }

            try
            {
                var result = await _projectService.CreateProjectAsync(dto, adminId);
                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(new { ProjectId = result.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin project creation failed.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the project");
            }
        }

        // Update project details as admin.
        [HttpPut("{projectId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProject(int projectId, UpdateProjectDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _projectService.UpdateProjectAsync(projectId, dto);
                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin project update failed for project id: {ProjectId}", projectId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the project");
            }
        }

        // Return all projects for admins.
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProjects()
        {
            try
            {
                var projects = await _projectService.GetAllAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin project list retrieval failed.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching projects");
            }
        }

        // Return projects assigned to the manager.
        [HttpGet("manager/{managerId:int}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetManagerProjects(int managerId)
        {
            if (!int.TryParse(User.FindFirstValue("UserId"), out var userId))
            {
                _logger.LogWarning("Manager project list retrieval failed due to missing user id claim.");
                return Unauthorized();
            }

            if (managerId != userId)
            {
                _logger.LogWarning("Manager project list retrieval denied for manager id mismatch. RouteId: {RouteId}, ClaimId: {ClaimId}", managerId, userId);
                return Forbid();
            }

            try
            {
                var projects = await _projectService.GetForManagerAsync(userId);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager project list retrieval failed. ManagerId: {ManagerId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching projects");
            }
        }

        // Return members assigned to a project for managers.
        [HttpGet("{projectId:int}/members")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetProjectMembers(int projectId)
        {
            if (!int.TryParse(User.FindFirstValue("UserId"), out var managerId))
            {
                _logger.LogWarning("Manager project member retrieval failed due to missing user id claim.");
                return Unauthorized();
            }

            try
            {
                var result = await _projectService.GetProjectMembersAsync(projectId, managerId);
                if (!result.Success)
                {
                    if (string.Equals(result.ErrorMessage, "FORBIDDEN", StringComparison.OrdinalIgnoreCase))
                    {
                        return Forbid();
                    }

                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager project member retrieval failed. ProjectId: {ProjectId}", projectId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching project members");
            }
        }

        // Assign a member to a project for managers.
        [HttpPost("{projectId:int}/members")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AddProjectMember(int projectId, AssignProjectMemberDTO dto)
        {
            if (!int.TryParse(User.FindFirstValue("UserId"), out var managerId))
            {
                _logger.LogWarning("Manager project member assignment failed due to missing user id claim.");
                return Unauthorized();
            }

            try
            {
                var result = await _projectService.AddProjectMemberAsync(projectId, managerId, dto.MemberId);
                if (!result.Success)
                {
                    if (string.Equals(result.ErrorMessage, "FORBIDDEN", StringComparison.OrdinalIgnoreCase))
                    {
                        return Forbid();
                    }

                    return BadRequest(result.ErrorMessage);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager project member assignment failed. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, dto.MemberId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while assigning project member");
            }
        }

        // Remove a member from a project for managers.
        [HttpDelete("{projectId:int}/members/{memberId:int}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> RemoveProjectMember(int projectId, int memberId)
        {
            if (!int.TryParse(User.FindFirstValue("UserId"), out var managerId))
            {
                _logger.LogWarning("Manager project member removal failed due to missing user id claim.");
                return Unauthorized();
            }

            try
            {
                var result = await _projectService.RemoveProjectMemberAsync(projectId, managerId, memberId);
                if (!result.Success)
                {
                    if (string.Equals(result.ErrorMessage, "FORBIDDEN", StringComparison.OrdinalIgnoreCase))
                    {
                        return Forbid();
                    }

                    return BadRequest(result.ErrorMessage);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager project member removal failed. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, memberId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing project member");
            }
        }
    }
}
