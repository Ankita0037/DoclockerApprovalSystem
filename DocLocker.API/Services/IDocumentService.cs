using DocLocker.Core.Models;

namespace DocLocker.API.Services
{
    public interface IDocumentService
    {
        Task<int> UploadAsync(UploadDocumentDTO model, int userId);
    }
}
