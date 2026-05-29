using DocLocker.API.Repositories;
using DocLocker.Core.Models;

namespace DocLocker.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        // Create a new user after validation.
        public async Task<(bool Success, string? ErrorMessage, int? UserId)> CreateUserAsync(CreateUserDTO dto, int currentUserId, bool currentUserIsSuperAdmin)
        {
            try
            {
                var normalizedEmail = dto.Email?.Trim().ToLowerInvariant();
                _logger.LogInformation("Admin user creation started for email: {Email}", normalizedEmail);

                if (string.IsNullOrWhiteSpace(normalizedEmail))
                {
                    _logger.LogWarning("Admin user creation failed due to empty email.");
                    return (false, "Email is required", null);
                }

                var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$");
                if (!passwordRegex.IsMatch(dto.Password ?? string.Empty))
                {
                    _logger.LogWarning("Admin user creation failed due to weak password.");
                    return (false, "Password must be at least 8 characters and include uppercase, lowercase, number, and special character", null);
                }

                if (await _userRepository.EmailExistsAsync(normalizedEmail))
                {
                    _logger.LogWarning("Admin user creation failed due to duplicate email: {Email}", normalizedEmail);
                    return (false, "Email already exists", null);
                }

                // This checks that only a super admin can create admin users or assign admin permissions.
                if ((dto.RoleId == 1 || dto.AllowUserManagement || dto.IsSuperAdmin) && !currentUserIsSuperAdmin)
                {
                    _logger.LogWarning("Admin user creation blocked because current admin cannot create admin users. AdminId: {AdminId}", currentUserId);
                    return (false, "Only Super Admin can create Admin users or assign user management", null);
                }

                if (dto.IsSuperAdmin && dto.RoleId != 1)
                {
                    _logger.LogWarning("Admin user creation failed because super admin flag requires admin role. AdminId: {AdminId}", currentUserId);
                    return (false, "Super Admin must have Admin role", null);
                }

                if (dto.RoleId != 1 && dto.RoleId != 2 && dto.RoleId != 3)
                {
                    _logger.LogWarning("Admin user creation failed due to invalid role: {RoleId}", dto.RoleId);
                    return (false, "Role must be Admin, Manager, or Member", null);
                }

                var roleExists = await _userRepository.RoleExistsAsync(dto.RoleId);
                if (!roleExists)
                {
                    _logger.LogWarning("Admin user creation failed because role not configured: {RoleId}", dto.RoleId);
                    return (false, "Role is not configured", null);
                }

                // This ensures the admin permission flags only apply to admin users.
                var isAdminRole = dto.RoleId == 1;
                var isSuperAdmin = isAdminRole && dto.IsSuperAdmin && currentUserIsSuperAdmin;
                var allowUserManagement = isAdminRole && (dto.AllowUserManagement || isSuperAdmin);

                var user = new User
                {
                    FullName = dto.FullName,
                    Email = normalizedEmail,
                    PhoneNumber = dto.PhoneNumber,
                    RoleId = dto.RoleId,
                    AllowUserManagement = allowUserManagement,
                    IsSuperAdmin = isSuperAdmin,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    IsActive = true
                };

                await _userRepository.AddAsync(user);

                _logger.LogInformation("Admin user created successfully: {Email}", dto.Email);
                _logger.LogInformation("Admin {AdminId} created user {UserId}", currentUserId, user.UserId);
                return (true, null, user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin user creation error for email: {Email}", dto.Email);
                throw;
            }
        }

        // Return a list of users with summary fields.
        public async Task<IReadOnlyList<UserSummaryDTO>> GetAllAsync()
        {
            _logger.LogInformation("Admin user list retrieval started.");

            var users = await _userRepository.GetAllWithRolesAsync();
            var results = users
                .Select(u => new UserSummaryDTO
                {
                    UserId = u.UserId,
                    Name = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    AllowUserManagement = u.AllowUserManagement,
                    RoleName = u.Role != null ? u.Role.Name : string.Empty,
                    IsActive = u.IsActive
                })
                .ToList();

            _logger.LogInformation("Admin user list retrieval completed. Count: {Count}", results.Count);
            return results;
        }

        // Update an existing user after validation.
        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int userId, UpdateUserDTO dto, int currentUserId, bool currentUserIsSuperAdmin)
        {
            try
            {
                _logger.LogInformation("Admin user update started for user id: {UserId}", userId);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Admin user update failed. User not found: {UserId}", userId);
                    return (false, "User not found");
                }

                // This prevents admins from changing their own role or permissions.
                if (currentUserId == userId)
                {
                    if (dto.RoleId != user.RoleId)
                    {
                        _logger.LogWarning("Admin self role update blocked. UserId: {UserId}", userId);
                        return (false, "Admins cannot update their own role");
                    }

                    if (dto.AllowUserManagement != user.AllowUserManagement)
                    {
                        _logger.LogWarning("Admin self permission update blocked. UserId: {UserId}", userId);
                        return (false, "Admins cannot update their own permissions");
                    }

                    if (dto.IsSuperAdmin != user.IsSuperAdmin)
                    {
                        _logger.LogWarning("Admin self super admin update blocked. UserId: {UserId}", userId);
                        return (false, "Admins cannot update their own super admin flag");
                    }
                }

                // This captures the existing admin state before updates.
                var wasAdminUser = user.RoleId == 1;

                // This ensures only super admins can update admin users or assign admin permissions.
                if ((wasAdminUser || dto.RoleId == 1 || dto.AllowUserManagement || dto.IsSuperAdmin) && !currentUserIsSuperAdmin)
                {
                    _logger.LogWarning("Admin user update blocked because current admin cannot update admin users. AdminId: {AdminId}, TargetId: {TargetId}", currentUserId, userId);
                    return (false, "Only Super Admin can update Admin users or assign user management");
                }

                if (dto.IsSuperAdmin && dto.RoleId != 1)
                {
                    _logger.LogWarning("Admin user update failed because super admin flag requires admin role. AdminId: {AdminId}, TargetId: {TargetId}", currentUserId, userId);
                    return (false, "Super Admin must have Admin role");
                }

                if (dto.RoleId != 1 && dto.RoleId != 2 && dto.RoleId != 3)
                {
                    _logger.LogWarning("Admin user update failed due to invalid role: {RoleId}", dto.RoleId);
                    return (false, "Role must be Admin, Manager, or Member");
                }

                var roleExists = await _userRepository.RoleExistsAsync(dto.RoleId);
                if (!roleExists)
                {
                    _logger.LogWarning("Admin user update failed because role not configured: {RoleId}", dto.RoleId);
                    return (false, "Role is not configured");
                }

                // This keeps admin permissions aligned with admin roles only.
                var isAdminRole = dto.RoleId == 1;
                var isSuperAdmin = isAdminRole && dto.IsSuperAdmin && currentUserIsSuperAdmin;
                var allowUserManagement = isAdminRole && (dto.AllowUserManagement || isSuperAdmin);

                user.FullName = dto.FullName;
                user.PhoneNumber = dto.PhoneNumber;
                user.RoleId = dto.RoleId;
                user.AllowUserManagement = allowUserManagement;
                user.IsSuperAdmin = isSuperAdmin;

                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("Admin user updated successfully. User id: {UserId}", userId);
                if (wasAdminUser)
                {
                    _logger.LogInformation("Admin {AdminId} updated admin {TargetId}", currentUserId, userId);
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin user update error for user id: {UserId}", userId);
                throw;
            }
        }

        // Toggle activation status for non-admin users.
        public async Task<(bool Success, string? ErrorMessage, bool? IsActive)> ToggleActiveAsync(int userId, int currentUserId, bool currentUserIsSuperAdmin)
        {
            try
            {
                _logger.LogInformation("Admin user activation toggle started for user id: {UserId}", userId);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Admin user activation toggle failed. User not found: {UserId}", userId);
                    return (false, "User not found", null);
                }

                // This prevents admins from deactivating themselves.
                if (currentUserId == userId)
                {
                    _logger.LogWarning("Admin self deactivation blocked. UserId: {UserId}", userId);
                    return (false, "Admins cannot deactivate themselves", null);
                }

                if (user.RoleId == 1)
                {
                    _logger.LogWarning("Admin user activation toggle blocked for admin user. UserId: {UserId}", userId);
                    return (false, "Admin users cannot be deactivated", null);
                }

                user.IsActive = !user.IsActive;
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("Admin user activation toggled. User id: {UserId}, IsActive: {IsActive}", userId, user.IsActive);
                return (true, null, user.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin user activation toggle error for user id: {UserId}", userId);
                throw;
            }
        }
    }
}
