using DocLocker.API.Services;
using DocLocker.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocLocker.API.Controllers
{
    // Document requests for managers.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentRequestsController : ControllerBase
    {
        private readonly IDocumentRequestService _documentRequestService;
        private readonly ILogger<DocumentRequestsController> _logger;

        // Initialize with service and logger.
        public DocumentRequestsController(IDocumentRequestService documentRequestService, ILogger<DocumentRequestsController> logger)
        {
            _documentRequestService = documentRequestService;
            _logger = logger;
        }

        // Create a new document request for a project member.
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateDocumentRequest(CreateDocumentRequestDTO dto)
        {
            // Log the incoming create request call.
            _logger.LogInformation("Manager request creation API called. ProjectId: {ProjectId}, MemberId: {MemberId}", dto.ProjectId, dto.MemberId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Manager request creation failed due to invalid model state. ProjectId: {ProjectId}, MemberId: {MemberId}", dto.ProjectId, dto.MemberId);
                return BadRequest(ModelState);
            }

            if (!int.TryParse(User.FindFirstValue("UserId"), out var managerId))
            {
                _logger.LogWarning("Manager document request creation failed due to missing user id claim.");
                return Unauthorized();
            }

            try
            {
                var result = await _documentRequestService.CreateAsync(dto, managerId);
                if (!result.Success)
                {
                    if (string.Equals(result.ErrorMessage, "FORBIDDEN", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Manager request creation forbidden. ManagerId: {ManagerId}, ProjectId: {ProjectId}", managerId, dto.ProjectId);
                        return Forbid();
                    }

                    _logger.LogWarning("Manager request creation failed. ManagerId: {ManagerId}, ProjectId: {ProjectId}, MemberId: {MemberId}, Error: {Error}", managerId, dto.ProjectId, dto.MemberId, result.ErrorMessage);
                    return BadRequest(result.ErrorMessage);
                }

                _logger.LogInformation("Manager request creation succeeded. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}", managerId, result.DocumentRequestId);
                return Ok(new { DocumentRequestId = result.DocumentRequestId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager document request creation failed. ProjectId: {ProjectId}, MemberId: {MemberId}", dto.ProjectId, dto.MemberId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the document request");
            }
        }

        // Return document requests created by the manager.
        [HttpGet("manager")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetManagerRequests()
        {
            if (!int.TryParse(User.FindFirstValue("UserId"), out var managerId))
            {
                _logger.LogWarning("Manager request list retrieval failed due to missing user id claim.");
                return Unauthorized();
            }

            try
            {
                var requests = await _documentRequestService.GetForManagerAsync(managerId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager request list retrieval failed. ManagerId: {ManagerId}", managerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching requests");
            }
        }

        // Return document requests assigned to the member.
        [HttpGet("member")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMemberRequests()
        {
            if (!int.TryParse(User.FindFirstValue("UserId"), out var memberId))
            {
                _logger.LogWarning("Member request list retrieval failed due to missing user id claim.");
                return Unauthorized();
            }

            try
            {
                var requests = await _documentRequestService.GetForMemberAsync(memberId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Member request list retrieval failed. MemberId: {MemberId}", memberId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching requests");
            }
        }

        // Update an existing pending document request.
        [HttpPut("{documentRequestId:int}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateDocumentRequest(int documentRequestId, UpdateDocumentRequestDTO dto)
        {
            _logger.LogInformation("Manager request update API called. DocumentRequestId: {DocumentRequestId}", documentRequestId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Manager request update failed due to invalid model state. DocumentRequestId: {DocumentRequestId}", documentRequestId);
                return BadRequest(ModelState);
            }

            if (!int.TryParse(User.FindFirstValue("UserId"), out var managerId))
            {
                _logger.LogWarning("Manager document request update failed due to missing user id claim.");
                return Unauthorized();
            }

            try
            {
                var result = await _documentRequestService.UpdateAsync(documentRequestId, dto, managerId);
                if (!result.Success)
                {
                    if (string.Equals(result.ErrorMessage, "FORBIDDEN", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Manager request update forbidden. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}", managerId, documentRequestId);
                        return Forbid();
                    }

                    _logger.LogWarning("Manager request update failed. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}, Error: {Error}", managerId, documentRequestId, result.ErrorMessage);
                    return BadRequest(result.ErrorMessage);
                }

                _logger.LogInformation("Manager request update succeeded. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}", managerId, documentRequestId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager document request update failed. DocumentRequestId: {DocumentRequestId}", documentRequestId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the document request");
            }
        }

        // Cancel a pending document request without deleting it.
        [HttpPatch("{documentRequestId:int}/cancel")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CancelDocumentRequest(int documentRequestId)
        {
            _logger.LogInformation("Manager request cancellation API called. DocumentRequestId: {DocumentRequestId}", documentRequestId);

            if (!int.TryParse(User.FindFirstValue("UserId"), out var managerId))
            {
                _logger.LogWarning("Manager document request cancellation failed due to missing user id claim.");
                return Unauthorized();
            }

            try
            {
                var result = await _documentRequestService.CancelAsync(documentRequestId, managerId);
                if (!result.Success)
                {
                    if (string.Equals(result.ErrorMessage, "FORBIDDEN", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Manager request cancellation forbidden. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}", managerId, documentRequestId);
                        return Forbid();
                    }

                    _logger.LogWarning("Manager request cancellation failed. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}, Error: {Error}", managerId, documentRequestId, result.ErrorMessage);
                    return BadRequest(result.ErrorMessage);
                }

                _logger.LogInformation("Manager request cancellation succeeded. ManagerId: {ManagerId}, DocumentRequestId: {DocumentRequestId}", managerId, documentRequestId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager document request cancellation failed. DocumentRequestId: {DocumentRequestId}", documentRequestId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while cancelling the document request");
            }
        }

        // Deleting document requests is intentionally not supported.
        [HttpDelete("{documentRequestId:int}")]
        [Authorize(Roles = "Manager")]
        public IActionResult DeleteDocumentRequest(int documentRequestId)
        {
            _logger.LogWarning("Delete document request API rejected. DocumentRequestId: {DocumentRequestId}", documentRequestId);
            return StatusCode(StatusCodes.Status405MethodNotAllowed, "Document requests cannot be deleted. Cancel pending requests instead.");
        }
    }
}
