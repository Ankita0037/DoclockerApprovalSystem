using DocLocker.API.Data;
using DocLocker.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

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
            return document.DocumentId;
        }

        public async Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId)
        {
            return await _context.Documents
                .AsNoTracking()
                .Where(document => document.UploadedByUserId == userId)
                .OrderByDescending(document => document.CreatedAt)
                .ToListAsync();
        }
    }
}
