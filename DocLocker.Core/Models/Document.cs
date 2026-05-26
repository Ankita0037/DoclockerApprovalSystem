namespace DocLocker.Core.Models
{
    public class Document
    {
        public int DocumentId { get; set; }

        public int DocumentRequestId { get; set; }
        public DocumentRequest DocumentRequest { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public int VersionNumber { get; set; }

        public bool IsLatest { get; set; }

        public int UploadedByUserId { get; set; }
        public User UploadedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<DocumentReview> Reviews { get; set; } = new List<DocumentReview>();
    }
}