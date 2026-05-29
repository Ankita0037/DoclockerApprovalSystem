using DocLocker.Core.Models;

namespace DocLocker.API.Repositories
{
    // Data access for document requests.
    public interface IDocumentRequestRepository
    {
        // Find a request status by name.
        Task<DocumentRequestStatus?> GetStatusByNameAsync(string name);

        // Find or create a request status by name.
        Task<DocumentRequestStatus> GetOrCreateStatusByNameAsync(string name);

        // Save a new request and its initial status history.
        Task<int> CreateAsync(DocumentRequest request, DocumentRequestStatusHistory history);

        // Find a request by id for workflow updates.
        Task<DocumentRequest?> GetByIdAsync(int documentRequestId);

        // Save request field updates.
        Task UpdateAsync(DocumentRequest request);

        // Save a status transition and its history entry.
        Task UpdateStatusAsync(DocumentRequest request, DocumentRequestStatusHistory history);

        // Return requests created by a manager.
        Task<IReadOnlyList<DocumentRequestSummaryDTO>> GetForManagerAsync(int managerId);

        // Return requests assigned to a member.
        Task<IReadOnlyList<MemberDocumentRequestSummaryDTO>> GetForMemberAsync(int memberId);
    }
}
