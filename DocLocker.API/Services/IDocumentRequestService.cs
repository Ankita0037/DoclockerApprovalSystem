using DocLocker.Core.Models;

namespace DocLocker.API.Services
{
    // Business logic for document requests.
    public interface IDocumentRequestService
    {
        // Create a new document request for a project member.
        Task<(bool Success, string? ErrorMessage, int? DocumentRequestId)> CreateAsync(CreateDocumentRequestDTO dto, int managerId);

        // Update editable fields on a pending request.
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(int documentRequestId, UpdateDocumentRequestDTO dto, int managerId);

        // Cancel a pending request without deleting it.
        Task<(bool Success, string? ErrorMessage)> CancelAsync(int documentRequestId, int managerId);

        // Return requests created by the manager.
        Task<IReadOnlyList<DocumentRequestSummaryDTO>> GetForManagerAsync(int managerId);

        // Return requests assigned to the member.
        Task<IReadOnlyList<MemberDocumentRequestSummaryDTO>> GetForMemberAsync(int memberId);
    }
}
