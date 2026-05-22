using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    public class User
    {
        public int UserId { get; set; }

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
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}