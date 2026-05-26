using Microsoft.AspNetCore.Http;

namespace DocLocker.Core.Models
{
    public class UploadDocumentDTO
    {
        public int DocumentRequestId { get; set; }
        public string FileName { get; set; }
        public IFormFile File { get; set; }
    }
}