using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    public class DocumentRequestStatus
    {
        public int DocumentRequestStatusId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        public ICollection<DocumentRequest> Requests { get; set; } = new List<DocumentRequest>();
    }
}
