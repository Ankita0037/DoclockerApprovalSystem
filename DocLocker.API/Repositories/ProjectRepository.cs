using DocLocker.API.Data;
using DocLocker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocLocker.API.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly DocLockerDbContext _context;
        private readonly ILogger<ProjectRepository> _logger;

        // Initialize with database context and logger.
        public ProjectRepository(DocLockerDbContext context, ILogger<ProjectRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Return a user by id.
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by id: {UserId}", userId);
                throw;
            }
        }

        // Create a project and assign the manager.
        public async Task<int> CreateProjectAsync(Project project, int managerId)
        {
            try
            {
                _context.Projects.Add(project);
                _context.ProjectManagers.Add(new ProjectManager
                {
                    Project = project,
                    ManagerId = managerId
                });

                await _context.SaveChangesAsync();
                return project.ProjectId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project: {ProjectName}", project.Name);
                throw;
            }
        }

        // Return a project with manager info.
        public async Task<Project?> GetProjectByIdAsync(int projectId)
        {
            try
            {
                return await _context.Projects
                    .Include(p => p.Managers)
                    .FirstOrDefaultAsync(p => p.ProjectId == projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project by id: {ProjectId}", projectId);
                throw;
            }
        }

        // Update project manager assignments.
        public async Task UpdateProjectAsync(Project project, int managerId)
        {
            try
            {
                var existingManagers = await _context.ProjectManagers
                    .Where(pm => pm.ProjectId == project.ProjectId)
                    .ToListAsync();

                if (existingManagers.All(pm => pm.ManagerId != managerId))
                {
                    _context.ProjectManagers.RemoveRange(existingManagers);
                    _context.ProjectManagers.Add(new ProjectManager
                    {
                        ProjectId = project.ProjectId,
                        ManagerId = managerId
                    });
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project: {ProjectId}", project.ProjectId);
                throw;
            }
        }

        // Return project summaries for admin views.
        public async Task<IReadOnlyList<ProjectSummaryDTO>> GetAllProjectSummariesAsync()
        {
            try
            {
                return await (from project in _context.Projects.AsNoTracking()
                              join manager in _context.ProjectManagers.AsNoTracking()
                                  on project.ProjectId equals manager.ProjectId
                              join user in _context.Users.AsNoTracking()
                                  on manager.ManagerId equals user.UserId
                              select new ProjectSummaryDTO
                              {
                                  ProjectId = project.ProjectId,
                                  Name = project.Name,
                                  ManagerName = user.FullName,
                                  Status = project.Status
                              }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project summaries.");
                throw;
            }
        }

        // Return project summaries for a manager.
        public async Task<IReadOnlyList<ProjectSummaryDTO>> GetProjectSummariesForManagerAsync(int managerId)
        {
            try
            {
                return await (from project in _context.Projects.AsNoTracking()
                              join manager in _context.ProjectManagers.AsNoTracking()
                                  on project.ProjectId equals manager.ProjectId
                              join user in _context.Users.AsNoTracking()
                                  on manager.ManagerId equals user.UserId
                              where manager.ManagerId == managerId
                              select new ProjectSummaryDTO
                              {
                                  ProjectId = project.ProjectId,
                                  Name = project.Name,
                                  ManagerName = user.FullName,
                                  Status = project.Status
                              }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manager project summaries. ManagerId: {ManagerId}", managerId);
                throw;
            }
        }

        // Check manager assignment for a project.
        public async Task<bool> IsManagerAssignedToProjectAsync(int projectId, int managerId)
        {
            try
            {
                return await _context.ProjectManagers
                    .AsNoTracking()
                    .AnyAsync(pm => pm.ProjectId == projectId && pm.ManagerId == managerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking manager assignment. ProjectId: {ProjectId}, ManagerId: {ManagerId}", projectId, managerId);
                throw;
            }
        }

        // Return assigned members for a project.
        public async Task<IReadOnlyList<UserSummaryDTO>> GetProjectMemberSummariesAsync(int projectId)
        {
            try
            {
                return await (from projectMember in _context.ProjectMembers.AsNoTracking()
                              join user in _context.Users.AsNoTracking()
                                  on projectMember.MemberId equals user.UserId
                              join role in _context.Roles.AsNoTracking()
                                  on user.RoleId equals role.RoleId
                              where projectMember.ProjectId == projectId
                              select new UserSummaryDTO
                              {
                                  UserId = user.UserId,
                                  Name = user.FullName,
                                  Email = user.Email,
                                  PhoneNumber = user.PhoneNumber,
                                  AllowUserManagement = user.AllowUserManagement,
                                  RoleName = role.Name,
                                  IsActive = user.IsActive
                              }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project members. ProjectId: {ProjectId}", projectId);
                throw;
            }
        }

        // Return available members not yet assigned.
        public async Task<IReadOnlyList<UserSummaryDTO>> GetAvailableMemberSummariesAsync(int projectId)
        {
            try
            {
                return await (from user in _context.Users.AsNoTracking()
                              join role in _context.Roles.AsNoTracking()
                                  on user.RoleId equals role.RoleId
                              where user.RoleId == 3
                              where !_context.ProjectMembers.Any(pm => pm.ProjectId == projectId && pm.MemberId == user.UserId)
                              select new UserSummaryDTO
                              {
                                  UserId = user.UserId,
                                  Name = user.FullName,
                                  Email = user.Email,
                                  PhoneNumber = user.PhoneNumber,
                                  AllowUserManagement = user.AllowUserManagement,
                                  RoleName = role.Name,
                                  IsActive = user.IsActive
                              }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available members. ProjectId: {ProjectId}", projectId);
                throw;
            }
        }

        // Check if member is already assigned.
        public async Task<bool> ProjectMemberExistsAsync(int projectId, int memberId)
        {
            try
            {
                return await _context.ProjectMembers
                    .AsNoTracking()
                    .AnyAsync(pm => pm.ProjectId == projectId && pm.MemberId == memberId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking project member assignment. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, memberId);
                throw;
            }
        }

        // Add a member assignment.
        public async Task AddProjectMemberAsync(ProjectMember projectMember)
        {
            try
            {
                _context.ProjectMembers.Add(projectMember);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding project member. ProjectId: {ProjectId}, MemberId: {MemberId}", projectMember.ProjectId, projectMember.MemberId);
                throw;
            }
        }

        // Remove a member assignment.
        public async Task RemoveProjectMemberAsync(int projectId, int memberId)
        {
            try
            {
                var projectMember = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.MemberId == memberId);
                if (projectMember == null)
                {
                    return;
                }

                _context.ProjectMembers.Remove(projectMember);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing project member. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, memberId);
                throw;
            }
        }
    }
}
