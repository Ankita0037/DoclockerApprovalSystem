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

        // Check if an email exists.
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

        // Check if a role exists.
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

        // Return a user by id.
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

        // Return all users with roles included.
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

        // Add a new user record.
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

        // Save pending changes.
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
