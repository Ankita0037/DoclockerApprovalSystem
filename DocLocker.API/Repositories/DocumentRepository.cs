using DocLocker.API.Data;
using DocLocker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocLocker.API.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly DocLockerDbContext _context;

        public DocumentRepository(DocLockerDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document.Id;
        }
    }
}
