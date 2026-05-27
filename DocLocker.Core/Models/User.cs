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
        public int RoleId { get; set; }
        public Role Role { get; set; }

        public bool AllowUserManagement { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
        public ICollection<ProjectManager> ProjectManagers { get; set; } = new List<ProjectManager>();
        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
        public ICollection<DocumentRequest> MemberDocumentRequests { get; set; } = new List<DocumentRequest>();
        public ICollection<DocumentRequest> ManagerDocumentRequests { get; set; } = new List<DocumentRequest>();
        public ICollection<Document> UploadedDocuments { get; set; } = new List<Document>();
        public ICollection<DocumentReview> Reviews { get; set; } = new List<DocumentReview>();
    }
}