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
    [Authorize(Roles = "Manager")]
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
    }
}
