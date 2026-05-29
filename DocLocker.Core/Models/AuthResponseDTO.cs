namespace DocLocker.Core.Models
{
    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool AllowUserManagement { get; set; }
        public bool IsSuperAdmin { get; set; }
    }
}