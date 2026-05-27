using DocLocker.API.Repositories;
using DocLocker.Core.Models;

namespace DocLocker.API.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;

        public DocumentService(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<int> UploadAsync(UploadDocumentDTO model, int userId)
        {
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

            return await _documentRepository.AddAsync(document);
        }

        // Return documents for the selected user.
        public async Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId)
        {
            return await _documentRepository.GetByUserIdAsync(userId);
        }
    }
}
