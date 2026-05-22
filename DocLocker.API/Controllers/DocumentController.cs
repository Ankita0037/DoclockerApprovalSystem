using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DocLocker.API.Data;
using DocLocker.Core.Models;
using System.Security.Claims;

namespace DocLocker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly DocLockerDbContext _context;

        public DocumentController(DocLockerDbContext context)
        {
            _context = context;
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

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid() + Path.GetExtension(model.File.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            var document = new Document
            {
                Title = model.Title,
                FilePath = fileName,
                Status = "Pending",
                UploadedByUserId = userId
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Document uploaded successfully", documentId = document.Id });
        }
    }
}