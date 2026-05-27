namespace DocLocker.Core.Models
{
    public class UserSummaryDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool AllowUserManagement { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}
