namespace DocLocker.Core.Models
{
    public class Document
    {
        public int DocumentId { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public int UploadedByUserId { get; set; }

        public int? AssignedManagerId { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}