using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
