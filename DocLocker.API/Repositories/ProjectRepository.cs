using DocLocker.API.Data;
using DocLocker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocLocker.API.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly DocLockerDbContext _context;
        private readonly ILogger<ProjectRepository> _logger;

        public ProjectRepository(DocLockerDbContext context, ILogger<ProjectRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

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
    }
}
