using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    public class UpdateUserDTO
    {
        [Required]
        [MinLength(3)]
        public string FullName { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be 10 digits")]
        public string PhoneNumber { get; set; }

        [Required]
        public int RoleId { get; set; }

        public bool AllowUserManagement { get; set; }

        public bool IsSuperAdmin { get; set; }
    }
}
