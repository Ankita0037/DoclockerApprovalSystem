using DocLocker.API.Data;
using DocLocker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocLocker.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DocLockerDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(DocLockerDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence: {Email}", email);
                throw;
            }
        }

        public async Task<bool> RoleExistsAsync(int roleId)
        {
            try
            {
                return await _context.Roles.AnyAsync(r => r.RoleId == roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role existence: {RoleId}", roleId);
                throw;
            }
        }

        public async Task<User?> GetByIdAsync(int userId)
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

        public async Task<IReadOnlyList<User>> GetAllWithRolesAsync()
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Include(u => u.Role)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user list.");
                throw;
            }
        }

        public async Task AddAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user: {Email}", user.Email);
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user changes.");
                throw;
            }
        }
    }
}
