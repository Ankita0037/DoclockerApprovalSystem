using DocLocker.Core.Models;

namespace DocLocker.API.Repositories
{
    public interface IDocumentRepository
    {
        // Add 
        Task<int> AddAsync(Document document);
        // Return documents uploaded
        Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId);
    }
}
