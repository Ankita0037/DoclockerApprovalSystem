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
            _logger.LogInformation("Admin project creation API called. ManagerId: {ManagerId}", dto.ManagerId);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin project creation failed due to invalid model state. ManagerId: {ManagerId}", dto.ManagerId);
                return BadRequest(ModelState);
            }

            if (!int.TryParse(User.FindFirstValue("UserId"), out var adminId))
            {
                _logger.LogWarning("Admin project creation failed due to missing admin id claim.");
                return Unauthorized();
            }

            var result = await _projectService.CreateProjectAsync(dto, adminId);
            if (!result.Success)
            {
                _logger.LogWarning("Admin project creation failed. AdminId: {AdminId}, Error: {Error}", adminId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            _logger.LogInformation("Admin project creation succeeded. AdminId: {AdminId}, ProjectId: {ProjectId}", adminId, result.ProjectId);
            return Ok(new { ProjectId = result.ProjectId });
        }

        // Update project details as admin.
        [HttpPut("{projectId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProject(int projectId, UpdateProjectDTO dto)
        {
            _logger.LogInformation("Admin project update API called. ProjectId: {ProjectId}, ManagerId: {ManagerId}", projectId, dto.ManagerId);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin project update failed due to invalid model state. ProjectId: {ProjectId}", projectId);
                return BadRequest(ModelState);
            }

            var result = await _projectService.UpdateProjectAsync(projectId, dto);
            if (!result.Success)
            {
                _logger.LogWarning("Admin project update failed. ProjectId: {ProjectId}, Error: {Error}", projectId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            _logger.LogInformation("Admin project update succeeded. ProjectId: {ProjectId}", projectId);
            return Ok();
        }

        // Return all projects for admins.
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProjects()
        {
            _logger.LogInformation("Admin project list API called.");
            var projects = await _projectService.GetAllAsync();
            _logger.LogInformation("Admin project list retrieval succeeded. Count: {Count}", projects.Count);
            return Ok(projects);
        }

        // Return projects assigned to the manager.
        [HttpGet("manager/{managerId:int}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetManagerProjects(int managerId)
        {
            _logger.LogInformation("Manager project list API called. RouteManagerId: {ManagerId}", managerId);
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

            var projects = await _projectService.GetForManagerAsync(userId);
            _logger.LogInformation("Manager project list retrieval succeeded. ManagerId: {ManagerId}, Count: {Count}", userId, projects.Count);
            return Ok(projects);
        }

        // Return members assigned to a project for managers.
        [HttpGet("{projectId:int}/members")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetProjectMembers(int projectId)
        {
            _logger.LogInformation("Manager project members API called. ProjectId: {ProjectId}", projectId);
            if (!int.TryParse(User.FindFirstValue("UserId"), out var managerId))
            {
                _logger.LogWarning("Manager project member retrieval failed due to missing user id claim.");
                return Unauthorized();
            }

            var result = await _projectService.GetProjectMembersAsync(projectId, managerId);
            if (!result.Success)
            {
                if (string.Equals(result.ErrorMessage, "FORBIDDEN", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Manager project member retrieval forbidden. ManagerId: {ManagerId}, ProjectId: {ProjectId}", managerId, projectId);
                    return Forbid();
                }

                _logger.LogWarning("Manager project member retrieval failed. ProjectId: {ProjectId}, Error: {Error}", projectId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            _logger.LogInformation("Manager project member retrieval succeeded. ProjectId: {ProjectId}", projectId);
            return Ok(result.Data);
        }

        // Assign a member to a project for managers.
        [HttpPost("{projectId:int}/members")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AddProjectMember(int projectId, AssignProjectMemberDTO dto)
        {
            _logger.LogInformation("Manager project member assignment API called. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, dto.MemberId);
            if (!int.TryParse(User.FindFirstValue("UserId"), out var managerId))
            {
                _logger.LogWarning("Manager project member assignment failed due to missing user id claim.");
                return Unauthorized();
            }

            var result = await _projectService.AddProjectMemberAsync(projectId, managerId, dto.MemberId);
            if (!result.Success)
            {
                if (string.Equals(result.ErrorMessage, "FORBIDDEN", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Manager project member assignment forbidden. ManagerId: {ManagerId}, ProjectId: {ProjectId}", managerId, projectId);
                    return Forbid();
                }

                _logger.LogWarning("Manager project member assignment failed. ProjectId: {ProjectId}, MemberId: {MemberId}, Error: {Error}", projectId, dto.MemberId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            _logger.LogInformation("Manager project member assignment succeeded. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, dto.MemberId);
            return Ok();
        }

        // Remove a member from a project for managers.
        [HttpDelete("{projectId:int}/members/{memberId:int}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> RemoveProjectMember(int projectId, int memberId)
        {
            _logger.LogInformation("Manager project member removal API called. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, memberId);
            if (!int.TryParse(User.FindFirstValue("UserId"), out var managerId))
            {
                _logger.LogWarning("Manager project member removal failed due to missing user id claim.");
                return Unauthorized();
            }

            var result = await _projectService.RemoveProjectMemberAsync(projectId, managerId, memberId);
            if (!result.Success)
            {
                if (string.Equals(result.ErrorMessage, "FORBIDDEN", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Manager project member removal forbidden. ManagerId: {ManagerId}, ProjectId: {ProjectId}", managerId, projectId);
                    return Forbid();
                }

                _logger.LogWarning("Manager project member removal failed. ProjectId: {ProjectId}, MemberId: {MemberId}, Error: {Error}", projectId, memberId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            _logger.LogInformation("Manager project member removal succeeded. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, memberId);
            return Ok();
        }
    }
}
