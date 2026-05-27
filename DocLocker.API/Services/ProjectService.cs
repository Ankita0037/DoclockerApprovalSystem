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

        // Create a new project for an admin.
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

        // Update project details for admins.
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

        // Return all project summaries for admins.
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

        // Return project summaries for the manager.
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

        // Return members assigned to a manager's project.
        public async Task<(bool Success, string? ErrorMessage, ProjectMembersViewDTO? Data)> GetProjectMembersAsync(int projectId, int managerId)
        {
            try
            {
                if (!await _projectRepository.IsManagerAssignedToProjectAsync(projectId, managerId))
                {
                    _logger.LogWarning("Manager project member retrieval denied. ProjectId: {ProjectId}, ManagerId: {ManagerId}", projectId, managerId);
                    return (false, "FORBIDDEN", null);
                }

                var assignedMembers = await _projectRepository.GetProjectMemberSummariesAsync(projectId);
                var availableMembers = await _projectRepository.GetAvailableMemberSummariesAsync(projectId);
                var data = new ProjectMembersViewDTO
                {
                    AssignedMembers = assignedMembers,
                    AvailableMembers = availableMembers
                };

                return (true, null, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager project member retrieval failed. ProjectId: {ProjectId}, ManagerId: {ManagerId}", projectId, managerId);
                throw;
            }
        }

        // Assign a member to a manager's project.
        public async Task<(bool Success, string? ErrorMessage)> AddProjectMemberAsync(int projectId, int managerId, int memberId)
        {
            try
            {
                if (!await _projectRepository.IsManagerAssignedToProjectAsync(projectId, managerId))
                {
                    _logger.LogWarning("Manager project member assignment denied. ProjectId: {ProjectId}, ManagerId: {ManagerId}", projectId, managerId);
                    return (false, "FORBIDDEN");
                }

                var user = await _projectRepository.GetUserByIdAsync(memberId);
                if (user == null)
                {
                    _logger.LogWarning("Project member assignment failed. Member not found: {MemberId}", memberId);
                    return (false, "Member not found");
                }

                if (user.RoleId != 3)
                {
                    _logger.LogWarning("Project member assignment failed. User is not a member: {MemberId}", memberId);
                    return (false, "User is not a member");
                }

                if (await _projectRepository.ProjectMemberExistsAsync(projectId, memberId))
                {
                    _logger.LogWarning("Project member assignment failed. Duplicate assignment. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, memberId);
                    return (false, "Member already assigned");
                }

                await _projectRepository.AddProjectMemberAsync(new ProjectMember
                {
                    ProjectId = projectId,
                    MemberId = memberId
                });

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager project member assignment failed. ProjectId: {ProjectId}, ManagerId: {ManagerId}, MemberId: {MemberId}", projectId, managerId, memberId);
                throw;
            }
        }

        // Remove a member from a manager's project.
        public async Task<(bool Success, string? ErrorMessage)> RemoveProjectMemberAsync(int projectId, int managerId, int memberId)
        {
            try
            {
                if (!await _projectRepository.IsManagerAssignedToProjectAsync(projectId, managerId))
                {
                    _logger.LogWarning("Manager project member removal denied. ProjectId: {ProjectId}, ManagerId: {ManagerId}", projectId, managerId);
                    return (false, "FORBIDDEN");
                }

                if (!await _projectRepository.ProjectMemberExistsAsync(projectId, memberId))
                {
                    _logger.LogWarning("Project member removal failed. Assignment not found. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, memberId);
                    return (false, "Member assignment not found");
                }

                await _projectRepository.RemoveProjectMemberAsync(projectId, memberId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager project member removal failed. ProjectId: {ProjectId}, ManagerId: {ManagerId}, MemberId: {MemberId}", projectId, managerId, memberId);
                throw;
            }
        }
    }
}
