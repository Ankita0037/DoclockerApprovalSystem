using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    public class DocumentReview
    {
        public int DocumentReviewId { get; set; }

        public int DocumentId { get; set; }
        public Document Document { get; set; }

        public int ManagerId { get; set; }
        public User Manager { get; set; }

        [Required, MaxLength(50)]
        public string Action { get; set; }

        [MaxLength(1000)]
        public string Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
