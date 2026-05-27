using DocLocker.API.Services;
using DocLocker.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocLocker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

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
    }
}
