using DocLocker.Core.Models;

namespace DocLocker.API.Repositories
{
    public interface IDocumentRepository
    {
        Task<int> AddAsync(Document document);
    }
}
