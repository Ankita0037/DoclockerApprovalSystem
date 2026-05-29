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

        // Find or create a request status by name.
        public async Task<DocumentRequestStatus> GetOrCreateStatusByNameAsync(string name)
        {
            try
            {
                var status = await _context.DocumentRequestStatuses
                    .FirstOrDefaultAsync(status => status.Name == name);

                if (status != null)
                {
                    return status;
                }

                status = new DocumentRequestStatus
                {
                    Name = name
                };

                _context.DocumentRequestStatuses.Add(status);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document request status created. StatusName: {StatusName}, StatusId: {StatusId}", name, status.DocumentRequestStatusId);
                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring document request status exists. StatusName: {StatusName}", name);
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

        // Find a request by id for workflow updates.
        public async Task<DocumentRequest?> GetByIdAsync(int documentRequestId)
        {
            try
            {
                return await _context.DocumentRequests
                    .Include(request => request.Status)
                    .FirstOrDefaultAsync(request => request.DocumentRequestId == documentRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document request. DocumentRequestId: {DocumentRequestId}", documentRequestId);
                throw;
            }
        }

        // Save request field updates.
        public async Task UpdateAsync(DocumentRequest request)
        {
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Document request updated. DocumentRequestId: {DocumentRequestId}", request.DocumentRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document request. DocumentRequestId: {DocumentRequestId}", request.DocumentRequestId);
                throw;
            }
        }

        // Save a status transition and its history entry.
        public async Task UpdateStatusAsync(DocumentRequest request, DocumentRequestStatusHistory history)
        {
            try
            {
                _context.DocumentRequestStatusHistories.Add(history);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Document request status updated. DocumentRequestId: {DocumentRequestId}, StatusId: {StatusId}", request.DocumentRequestId, request.DocumentRequestStatusId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document request status. DocumentRequestId: {DocumentRequestId}", request.DocumentRequestId);
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
                        Description = request.Description,
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

        // Return requests assigned to a member.
        public async Task<IReadOnlyList<MemberDocumentRequestSummaryDTO>> GetForMemberAsync(int memberId)
        {
            try
            {
                return await _context.DocumentRequests
                    .AsNoTracking()
                    .Where(request => request.MemberId == memberId)
                    .Select(request => new MemberDocumentRequestSummaryDTO
                    {
                        DocumentRequestId = request.DocumentRequestId,
                        ProjectName = request.Project.Name,
                        Title = request.Title,
                        Description = request.Description,
                        Status = request.Status.Name,
                        DueDate = request.DueDate
                    })
                    .OrderByDescending(request => request.DocumentRequestId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving member document requests. MemberId: {MemberId}", memberId);
                throw;
            }
        }
    }
}
