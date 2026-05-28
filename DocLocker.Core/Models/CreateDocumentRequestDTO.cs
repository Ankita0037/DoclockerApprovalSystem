using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    // Data needed to create a document request.
    public class CreateDocumentRequestDTO
    {
        // Project where the request belongs.
        [Required]
        public int ProjectId { get; set; }

        // Member who will receive the request.
        [Required]
        public int MemberId { get; set; }

        // Title shown for the request.
        [Required, MaxLength(200)]
        public string Title { get; set; }

        // Optional details for the request.
        [MaxLength(1000)]
        public string? Description { get; set; }

        // Optional due date for the request.
        public DateTime? DueDate { get; set; }
    }
}
