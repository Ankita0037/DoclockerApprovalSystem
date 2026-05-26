using DocLocker.Core.Models;

namespace DocLocker.API.Repositories
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> RoleExistsAsync(int roleId);
        Task<User?> GetByIdAsync(int userId);
        Task<IReadOnlyList<User>> GetAllWithRolesAsync();
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
