using DocLocker.Core.Models;

namespace DocLocker.Web.Models
{
    // View model for manager document request list.
    public class ManagerDocumentRequestsViewModel
    {
        // Current status filter for the page.
        public string? StatusFilter { get; set; }

        // Requests to display in the table.
        public IReadOnlyList<DocumentRequestSummaryDTO> Requests { get; set; } = Array.Empty<DocumentRequestSummaryDTO>();
    }
}
