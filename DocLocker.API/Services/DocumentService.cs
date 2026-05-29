using DocLocker.API.Repositories;
using DocLocker.Core.Models;

namespace DocLocker.API.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(IDocumentRepository documentRepository, ILogger<DocumentService> logger)
        {
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public async Task<int> UploadAsync(UploadDocumentDTO model, int userId)
        {
            _logger.LogInformation("Document upload started. DocumentRequestId: {DocumentRequestId}, UserId: {UserId}", model.DocumentRequestId, userId);
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
                DocumentRequestId = model.DocumentRequestId,
                FileName = model.FileName,
                FilePath = fileName,
                VersionNumber = 1,
                IsLatest = true,
                UploadedByUserId = userId
            };

            var documentId = await _documentRepository.AddAsync(document);
            _logger.LogInformation("Document upload completed. DocumentId: {DocumentId}, UserId: {UserId}", documentId, userId);
            return documentId;
        }

        // Return documents for the selected user.
        public async Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId)
        {
            _logger.LogInformation("Document list retrieval started. UserId: {UserId}", userId);
            return await _documentRepository.GetByUserIdAsync(userId);
        }
    }
}
