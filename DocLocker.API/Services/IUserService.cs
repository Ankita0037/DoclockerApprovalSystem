using DocLocker.Core.Models;

namespace DocLocker.API.Services
{
    public interface IUserService
    {
        Task<(bool Success, string? ErrorMessage, int? UserId)> CreateUserAsync(CreateUserDTO dto, int currentUserId, bool currentUserIsSuperAdmin);
        Task<IReadOnlyList<UserSummaryDTO>> GetAllAsync();
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(int userId, UpdateUserDTO dto, int currentUserId, bool currentUserIsSuperAdmin);
        Task<(bool Success, string? ErrorMessage, bool? IsActive)> ToggleActiveAsync(int userId, int currentUserId, bool currentUserIsSuperAdmin);
    }
}
