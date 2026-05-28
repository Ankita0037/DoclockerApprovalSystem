using DocLocker.Core.Models;

namespace DocLocker.Web.Models
{
    // View model for member document request list.
    public class MemberDocumentRequestsViewModel
    {
        // Requests to display in the table.
        public IReadOnlyList<MemberDocumentRequestSummaryDTO> Requests { get; set; } = Array.Empty<MemberDocumentRequestSummaryDTO>();
    }
}
