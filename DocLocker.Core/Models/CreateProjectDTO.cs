using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    public class CreateProjectDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int ManagerId { get; set; }
    }
}
