using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DocLocker.API.Services;
using DocLocker.Core.Models;
using System.Security.Claims;

namespace DocLocker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentDTO model)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest("File is missing");

            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Title is required");

            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized("User ID not found in token");

            var documentId = await _documentService.UploadAsync(model, userId);

            return Ok(new { message = "Document uploaded successfully", documentId });
        }
    }
}