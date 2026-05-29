using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    public class CreateUserDTO
    {
        [Required]
        [MinLength(3)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be 10 digits")]
        public string PhoneNumber { get; set; }

        [Required]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must be at least 8 characters and include 1 uppercase letter and 1 number")]
        public string Password { get; set; }

        [Required]
        public int RoleId { get; set; }

        public bool AllowUserManagement { get; set; }

        public bool IsSuperAdmin { get; set; }
    }
}
