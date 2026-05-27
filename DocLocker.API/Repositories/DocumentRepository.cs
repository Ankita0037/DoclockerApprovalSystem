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

        // Database context.
        public DocumentRepository(DocLockerDbContext context)
        {
            _context = context;
        }

        // Insert a new document record.
        public async Task<int> AddAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document.DocumentId;
        }

        // Fetch documents uploaded by a user.
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
