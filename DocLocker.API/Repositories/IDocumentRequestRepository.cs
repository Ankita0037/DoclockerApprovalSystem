using DocLocker.Core.Models;

namespace DocLocker.API.Repositories
{
    // Data access for document requests.
    public interface IDocumentRequestRepository
    {
        // Find a request status by name.
        Task<DocumentRequestStatus?> GetStatusByNameAsync(string name);

        // Save a new request and its initial status history.
        Task<int> CreateAsync(DocumentRequest request, DocumentRequestStatusHistory history);

        // Return requests created by a manager.
        Task<IReadOnlyList<DocumentRequestSummaryDTO>> GetForManagerAsync(int managerId);
    }
}
