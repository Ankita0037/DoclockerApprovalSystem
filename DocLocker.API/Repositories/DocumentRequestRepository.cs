using DocLocker.API.Data;
using DocLocker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocLocker.API.Repositories
{
    // Repository for document requests and status history.
    public class DocumentRequestRepository : IDocumentRequestRepository
    {
        private readonly DocLockerDbContext _context;
        private readonly ILogger<DocumentRequestRepository> _logger;

        // Initialize with database context and logger.
        public DocumentRequestRepository(DocLockerDbContext context, ILogger<DocumentRequestRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Find a request status by name.
        public async Task<DocumentRequestStatus?> GetStatusByNameAsync(string name)
        {
            try
            {
                return await _context.DocumentRequestStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(status => status.Name == name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document request status by name: {StatusName}", name);
                throw;
            }
        }

        // Save a new request and its initial status history.
        public async Task<int> CreateAsync(DocumentRequest request, DocumentRequestStatusHistory history)
        {
            try
            {
                // Track the create operation for a document request and history entry.
                _logger.LogInformation("Creating document request for project id: {ProjectId}, member id: {MemberId}", request.ProjectId, request.MemberId);
                _context.DocumentRequests.Add(request);
                _context.DocumentRequestStatusHistories.Add(history);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Document request created with id: {DocumentRequestId}", request.DocumentRequestId);
                return request.DocumentRequestId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document request for project id: {ProjectId}", request.ProjectId);
                throw;
            }
        }

        // Return requests created by a manager.
        public async Task<IReadOnlyList<DocumentRequestSummaryDTO>> GetForManagerAsync(int managerId)
        {
            try
            {
                return await _context.DocumentRequests
                    .AsNoTracking()
                    .Where(request => request.RequestedByManagerId == managerId)
                    .Select(request => new DocumentRequestSummaryDTO
                    {
                        DocumentRequestId = request.DocumentRequestId,
                        ProjectName = request.Project.Name,
                        MemberName = request.Member.FullName,
                        Title = request.Title,
                        Status = request.Status.Name,
                        DueDate = request.DueDate
                    })
                    .OrderByDescending(request => request.DocumentRequestId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manager document requests. ManagerId: {ManagerId}", managerId);
                throw;
            }
        }
    }
}
