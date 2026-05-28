namespace DocLocker.Core.Models
{
    // Summary data for member document requests.
    public class MemberDocumentRequestSummaryDTO
    {
        // Unique id of the request.
        public int DocumentRequestId { get; set; }

        // Project name for display.
        public string ProjectName { get; set; } = string.Empty;

        // Request title.
        public string Title { get; set; } = string.Empty;

        // Request description.
        public string Description { get; set; } = string.Empty;

        // Status name for the request.
        public string Status { get; set; } = string.Empty;

        // Due date for the request.
        public DateTime? DueDate { get; set; }
    }
}
