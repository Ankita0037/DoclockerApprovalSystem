using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    public class DocumentRequest
    {
        public int DocumentRequestId { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int MemberId { get; set; } // ID of the member making the request
        public User Member { get; set; } // User object representing the member

        public int RequestedByManagerId { get; set; } // ID of the manager who requested
        public User RequestedByManager { get; set; } // User object representing the manager

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public int DocumentRequestStatusId { get; set; } // Status ID of the document request
        public DocumentRequestStatus Status { get; set; } // Current status of the document request

        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<DocumentRequestStatusHistory> StatusHistory { get; set; } = new List<DocumentRequestStatusHistory>();
    }
}
