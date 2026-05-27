using DocLocker.Core.Models;

namespace DocLocker.API.Services
{
    public interface IProjectService
    {
        Task<(bool Success, string? ErrorMessage, int? ProjectId)> CreateProjectAsync(CreateProjectDTO dto, int adminId);
        Task<(bool Success, string? ErrorMessage)> UpdateProjectAsync(int projectId, UpdateProjectDTO dto);
        Task<IReadOnlyList<ProjectSummaryDTO>> GetAllAsync();
        Task<IReadOnlyList<ProjectSummaryDTO>> GetForManagerAsync(int managerId);
    }
}
