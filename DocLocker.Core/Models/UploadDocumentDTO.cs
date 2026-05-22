using Microsoft.AspNetCore.Http;

namespace DocLocker.Core.Models
{
    public class UploadDocumentDTO
    {
        public string Title { get; set; }
        public IFormFile File { get; set; }
    }
}