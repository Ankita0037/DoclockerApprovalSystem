using DocLocker.API.Repositories;
using DocLocker.Core.Models;

namespace DocLocker.API.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(IProjectRepository projectRepository, ILogger<ProjectService> logger)
        {
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task<(bool Success, string? ErrorMessage, int? ProjectId)> CreateProjectAsync(CreateProjectDTO dto, int adminId)
        {
            try
            {
                var normalizedName = dto.Name?.Trim();
                _logger.LogInformation("Admin project creation started. AdminId: {AdminId}", adminId);

                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    _logger.LogWarning("Admin project creation failed due to empty name.");
                    return (false, "Project name is required", null);
                }

                var manager = await _projectRepository.GetUserByIdAsync(dto.ManagerId);
                if (manager == null)
                {
                    _logger.LogWarning("Admin project creation failed. Manager not found: {ManagerId}", dto.ManagerId);
                    return (false, "Manager not found", null);
                }

                if (manager.RoleId != 2)
                {
                    _logger.LogWarning("Admin project creation failed. User is not a manager: {ManagerId}", dto.ManagerId);
                    return (false, "User is not a manager", null);
                }

                var project = new Project
                {
                    Name = normalizedName,
                    CreatedByAdminId = adminId,
                    Status = "Active",
                    IsActive = true
                };

                var projectId = await _projectRepository.CreateProjectAsync(project, dto.ManagerId);

                _logger.LogInformation("Admin project created successfully. ProjectId: {ProjectId}", projectId);
                return (true, null, projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin project creation error for manager id: {ManagerId}", dto.ManagerId);
                throw;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateProjectAsync(int projectId, UpdateProjectDTO dto)
        {
            try
            {
                var normalizedName = dto.Name?.Trim();
                _logger.LogInformation("Admin project update started. ProjectId: {ProjectId}", projectId);

                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    _logger.LogWarning("Admin project update failed due to empty name. ProjectId: {ProjectId}", projectId);
                    return (false, "Project name is required");
                }

                var project = await _projectRepository.GetProjectByIdAsync(projectId);
                if (project == null)
                {
                    _logger.LogWarning("Admin project update failed. Project not found: {ProjectId}", projectId);
                    return (false, "Project not found");
                }

                var manager = await _projectRepository.GetUserByIdAsync(dto.ManagerId);
                if (manager == null)
                {
                    _logger.LogWarning("Admin project update failed. Manager not found: {ManagerId}", dto.ManagerId);
                    return (false, "Manager not found");
                }

                if (manager.RoleId != 2)
                {
                    _logger.LogWarning("Admin project update failed. User is not a manager: {ManagerId}", dto.ManagerId);
                    return (false, "User is not a manager");
                }

                project.Name = normalizedName;

                await _projectRepository.UpdateProjectAsync(project, dto.ManagerId);

                _logger.LogInformation("Admin project updated successfully. ProjectId: {ProjectId}", projectId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin project update error for project id: {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<IReadOnlyList<ProjectSummaryDTO>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Admin project list retrieval started.");
                var projects = await _projectRepository.GetAllProjectSummariesAsync();
                _logger.LogInformation("Admin project list retrieval completed. Count: {Count}", projects.Count);
                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin project list retrieval failed.");
                throw;
            }
        }

        public async Task<IReadOnlyList<ProjectSummaryDTO>> GetForManagerAsync(int managerId)
        {
            try
            {
                _logger.LogInformation("Manager project list retrieval started. ManagerId: {ManagerId}", managerId);
                var projects = await _projectRepository.GetProjectSummariesForManagerAsync(managerId);
                _logger.LogInformation("Manager project list retrieval completed. Count: {Count}", projects.Count);
                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager project list retrieval failed. ManagerId: {ManagerId}", managerId);
                throw;
            }
        }
    }
}
