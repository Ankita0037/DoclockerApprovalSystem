namespace DocLocker.Core.Models
{
    public class Document
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string FilePath { get; set; }

        public string Status { get; set; } // Pending, Approved, Rejected, Revision

        public int UploadedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}