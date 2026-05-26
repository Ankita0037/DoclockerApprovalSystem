using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    public class DocumentRequestStatusHistory
    {
        public int Id { get; set; }

        public int DocumentRequestId { get; set; }
        public DocumentRequest DocumentRequest { get; set; }

        public int StatusId { get; set; }
        public DocumentRequestStatus Status { get; set; }

        public int ChangedByUserId { get; set; }
        public User ChangedByUser { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}
