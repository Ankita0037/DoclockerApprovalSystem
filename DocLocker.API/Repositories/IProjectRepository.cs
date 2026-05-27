using DocLocker.Core.Models;

namespace DocLocker.API.Repositories
{
    public interface IProjectRepository
    {

        Task<User?> GetUserByIdAsync(int userId);
        Task<Project?> GetProjectByIdAsync(int projectId);
        Task<int> CreateProjectAsync(Project project, int managerId);
        Task UpdateProjectAsync(Project project, int managerId);
        Task<IReadOnlyList<ProjectSummaryDTO>> GetAllProjectSummariesAsync();
        Task<IReadOnlyList<ProjectSummaryDTO>> GetProjectSummariesForManagerAsync(int managerId);
        Task<bool> IsManagerAssignedToProjectAsync(int projectId, int managerId);
        Task<IReadOnlyList<UserSummaryDTO>> GetProjectMemberSummariesAsync(int projectId);
        Task<IReadOnlyList<UserSummaryDTO>> GetAvailableMemberSummariesAsync(int projectId);
        Task<bool> ProjectMemberExistsAsync(int projectId, int memberId);
        Task AddProjectMemberAsync(ProjectMember projectMember);
        Task RemoveProjectMemberAsync(int projectId, int memberId);
    }
}
