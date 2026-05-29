using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    // Editable fields for a pending document request.
    public class UpdateDocumentRequestDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
