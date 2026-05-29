using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DocLocker.API.Services;
using DocLocker.Core.Models;
using System.Security.Claims;

namespace DocLocker.API.Controllers
{
    // Document upload and retrieval.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentController> _logger;

        // Initialize with document service.
        public DocumentController(IDocumentService documentService, ILogger<DocumentController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        // Upload a document file for a request.
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentDTO model)
        {
            _logger.LogInformation("Document upload request started. DocumentRequestId: {DocumentRequestId}", model.DocumentRequestId);

            if (model.File == null || model.File.Length == 0)
            {
                _logger.LogWarning("Document upload validation failed due to missing file. DocumentRequestId: {DocumentRequestId}", model.DocumentRequestId);
                return BadRequest("File is missing");
            }

            if (string.IsNullOrWhiteSpace(model.FileName))
            {
                _logger.LogWarning("Document upload validation failed due to missing file name. DocumentRequestId: {DocumentRequestId}", model.DocumentRequestId);
                return BadRequest("File name is required");
            }

            if (model.DocumentRequestId <= 0)
            {
                _logger.LogWarning("Document upload validation failed due to missing request id.");
                return BadRequest("Document request is required");
            }

            // Get current user ID from JWT token.
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("Document upload unauthorized due to missing user id claim.");
                return Unauthorized("User ID not found in token");
            }

            var documentId = await _documentService.UploadAsync(model, userId);

            _logger.LogInformation("Document upload completed. DocumentId: {DocumentId}, UserId: {UserId}", documentId, userId);
            return Ok(new { message = "Document uploaded successfully", documentId });
        }

        // Return documents for a specific user.
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            _logger.LogInformation("Document list request started. UserId: {UserId}", userId);
            var documents = await _documentService.GetByUserIdAsync(userId);
            _logger.LogInformation("Document list request completed. UserId: {UserId}, Count: {Count}", userId, documents.Count);
            return Ok(documents);
        }
    }
}