using DocLocker.API.Repositories;
using DocLocker.Core.Models;

namespace DocLocker.API.Services
{
    // Service for creating document requests.
    public class DocumentRequestService : IDocumentRequestService
    {
        private const string PendingStatusName = "Pending";
        private const string CancelledStatusName = "Cancelled";
        private readonly IDocumentRequestRepository _documentRequestRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<DocumentRequestService> _logger;

        // Initialize with repositories and logger.
        public DocumentRequestService(
            IDocumentRequestRepository documentRequestRepository,
            IProjectRepository projectRepository,
            ILogger<DocumentRequestService> logger)
        {
            _documentRequestRepository = documentRequestRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        // Create a new document request for a project member.
        public async Task<(bool Success, string? ErrorMessage, int? DocumentRequestId)> CreateAsync(CreateDocumentRequestDTO dto, int managerId)
        {
            try
            {
                var normalizedTitle = dto.Title?.Trim();
                var normalizedDescription = dto.Description?.Trim();

                _logger.LogInformation("Manager document request creation started. ManagerId: {ManagerId}, ProjectId: {ProjectId}, MemberId: {MemberId}", managerId, dto.ProjectId, dto.MemberId);

                if (string.IsNullOrWhiteSpace(normalizedTitle))
                {
                    _logger.LogWarning("Manager document request creation failed due to empty title. ProjectId: {ProjectId}", dto.ProjectId);
                    return (false, "Title is required", null);
                }

                var project = await _projectRepository.GetProjectByIdAsync(dto.ProjectId);
                if (project == null)
                {
                    _logger.LogWarning("Manager document request creation failed. Project not found: {ProjectId}", dto.ProjectId);
                    return (false, "Project not found", null);
                }

                _logger.LogInformation("Project validated for request creation. ProjectId: {ProjectId}", dto.ProjectId);

                if (!await _projectRepository.IsManagerAssignedToProjectAsync(dto.ProjectId, managerId))
                {
                    _logger.LogWarning("Manager document request creation denied. ManagerId: {ManagerId}, ProjectId: {ProjectId}", managerId, dto.ProjectId);
                    return (false, "FORBIDDEN", null);
                }

                _logger.LogInformation("Manager assignment validated for request creation. ManagerId: {ManagerId}, ProjectId: {ProjectId}", managerId, dto.ProjectId);

                if (!await _projectRepository.ProjectMemberExistsAsync(dto.ProjectId, dto.MemberId))
                {
                    _logger.LogWarning("Manager document request creation failed. Member not assigned to project. ProjectId: {ProjectId}, MemberId: {MemberId}", dto.ProjectId, dto.MemberId);
                    return (false, "Member is not assigned to project", null);
                }

                _logger.LogInformation("Member assignment validated for request creation. ProjectId: {ProjectId}, MemberId: {MemberId}", dto.ProjectId, dto.MemberId);

                var pendingStatus = await _documentRequestRepository.GetStatusByNameAsync(PendingStatusName);
                if (pendingStatus == null)
                {
                    _logger.LogError("Manager document request creation failed. Pending status missing.");
                    return (false, "Pending status not found", null);
                }

                _logger.LogInformation("Pending status loaded for request creation. StatusId: {StatusId}", pendingStatus.DocumentRequestStatusId);

                var request = new DocumentRequest
                {
                    ProjectId = dto.ProjectId,
                    MemberId = dto.MemberId,
                    RequestedByManagerId = managerId,
                    Title = normalizedTitle!,
                    Description = normalizedDescription ?? string.Empty,
                    DueDate = dto.DueDate,
                    DocumentRequestStatusId = pendingStatus.DocumentRequestStatusId,
                    CreatedAt = DateTime.Now
                };

                var history = new DocumentRequestStatusHistory
                {
                    DocumentRequest = request,
                    StatusId = pendingStatus.DocumentRequestStatusId,
                    ChangedByUserId = managerId,
                    ChangedAt = DateTime.Now,
                    Notes = string.Empty
                };

                var requestId = await _documentRequestRepository.CreateAsync(request, history);

                _logger.LogInformation("Manager document request created successfully. DocumentRequestId: {DocumentRequestId}", requestId);
                return (true, null, requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager document request creation failed. ProjectId: {ProjectId}, MemberId: {MemberId}", dto.ProjectId, dto.MemberId);
                throw;
            }
        }

        // Update editable fields on a pending request.
        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int documentRequestId, UpdateDocumentRequestDTO dto, int managerId)
        {
            try
            {
                var normalizedTitle = dto.Title?.Trim();
                var normalizedDescription = dto.Description?.Trim();

                _logger.LogInformation("Manager document request update started. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}", managerId, documentRequestId);

                if (string.IsNullOrWhiteSpace(normalizedTitle))
                {
                    _logger.LogWarning("Document request update failed due to empty title. DocumentRequestId: {DocumentRequestId}", documentRequestId);
                    return (false, "Title is required");
                }

                if (dto.DueDate.HasValue && dto.DueDate.Value.Date < DateTime.Today)
                {
                    _logger.LogWarning("Document request update failed due to past due date. DocumentRequestId: {DocumentRequestId}", documentRequestId);
                    return (false, "Due date cannot be in the past");
                }

                var request = await _documentRequestRepository.GetByIdAsync(documentRequestId);
                if (request == null)
                {
                    _logger.LogWarning("Document request update failed. Request not found: {DocumentRequestId}", documentRequestId);
                    return (false, "Document request not found");
                }

                if (request.RequestedByManagerId != managerId)
                {
                    _logger.LogWarning("Document request update forbidden. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}", managerId, documentRequestId);
                    return (false, "FORBIDDEN");
                }

                if (!string.Equals(request.Status.Name, PendingStatusName, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Document request update blocked because request is not pending. DocumentRequestId: {DocumentRequestId}, Status: {Status}", documentRequestId, request.Status.Name);
                    return (false, "Only pending requests can be edited");
                }

                request.Title = normalizedTitle!;
                request.Description = normalizedDescription ?? string.Empty;
                request.DueDate = dto.DueDate;

                await _documentRequestRepository.UpdateAsync(request);

                _logger.LogInformation("Manager document request update completed. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}", managerId, documentRequestId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager document request update failed. DocumentRequestId: {DocumentRequestId}", documentRequestId);
                throw;
            }
        }

        // Cancel a pending request without deleting it.
        public async Task<(bool Success, string? ErrorMessage)> CancelAsync(int documentRequestId, int managerId)
        {
            try
            {
                _logger.LogInformation("Manager document request cancellation started. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}", managerId, documentRequestId);

                var request = await _documentRequestRepository.GetByIdAsync(documentRequestId);
                if (request == null)
                {
                    _logger.LogWarning("Document request cancellation failed. Request not found: {DocumentRequestId}", documentRequestId);
                    return (false, "Document request not found");
                }

                if (request.RequestedByManagerId != managerId)
                {
                    _logger.LogWarning("Document request cancellation forbidden. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}", managerId, documentRequestId);
                    return (false, "FORBIDDEN");
                }

                if (!string.Equals(request.Status.Name, PendingStatusName, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Document request cancellation blocked because request is not pending. DocumentRequestId: {DocumentRequestId}, Status: {Status}", documentRequestId, request.Status.Name);
                    return (false, "Only pending requests can be cancelled");
                }

                var cancelledStatus = await _documentRequestRepository.GetOrCreateStatusByNameAsync(CancelledStatusName);

                request.DocumentRequestStatusId = cancelledStatus.DocumentRequestStatusId;

                var history = new DocumentRequestStatusHistory
                {
                    DocumentRequestId = request.DocumentRequestId,
                    StatusId = cancelledStatus.DocumentRequestStatusId,
                    ChangedByUserId = managerId,
                    ChangedAt = DateTime.Now,
                    Notes = "Request cancelled by manager"
                };

                await _documentRequestRepository.UpdateStatusAsync(request, history);

                _logger.LogInformation("Manager document request cancellation completed. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}", managerId, documentRequestId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager document request cancellation failed. DocumentRequestId: {DocumentRequestId}", documentRequestId);
                throw;
            }
        }

        // Return requests created by the manager.
        public async Task<IReadOnlyList<DocumentRequestSummaryDTO>> GetForManagerAsync(int managerId)
        {
            try
            {
                _logger.LogInformation("Manager request list retrieval started. ManagerId: {ManagerId}", managerId);
                var requests = await _documentRequestRepository.GetForManagerAsync(managerId);
                _logger.LogInformation("Manager request list retrieval completed. ManagerId: {ManagerId}, Count: {Count}", managerId, requests.Count);
                return requests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager request list retrieval failed. ManagerId: {ManagerId}", managerId);
                throw;
            }
        }

        // Return requests assigned to the member.
        public async Task<IReadOnlyList<MemberDocumentRequestSummaryDTO>> GetForMemberAsync(int memberId)
        {
            try
            {
                _logger.LogInformation("Member request list retrieval started. MemberId: {MemberId}", memberId);
                var requests = await _documentRequestRepository.GetForMemberAsync(memberId);
                _logger.LogInformation("Member request list retrieval completed. MemberId: {MemberId}, Count: {Count}", memberId, requests.Count);
                return requests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Member request list retrieval failed. MemberId: {MemberId}", memberId);
                throw;
            }
        }
    }
}
