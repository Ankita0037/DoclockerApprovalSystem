using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    // Data needed to create a document request.
    public class CreateDocumentRequestDTO
    {
        // Project where the request belongs.
        [Range(1, int.MaxValue, ErrorMessage = "Project is required")]
        public int ProjectId { get; set; }

        // Member who will receive the request.
        [Range(1, int.MaxValue, ErrorMessage = "Please select a member")]
        public int MemberId { get; set; }

        // Title shown for the request.
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        // Optional details for the request.
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        // Optional due date for the request.
        public DateTime? DueDate { get; set; }
    }
}
