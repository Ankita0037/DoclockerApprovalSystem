using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    public class Project
    {
        public int ProjectId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        public int CreatedByAdminId { get; set; }
        public User CreatedByAdmin { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ProjectManager> Managers { get; set; } = new List<ProjectManager>();
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public ICollection<DocumentRequest> DocumentRequests { get; set; } = new List<DocumentRequest>();
    }
}
