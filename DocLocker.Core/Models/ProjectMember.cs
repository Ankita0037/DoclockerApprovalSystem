namespace DocLocker.Core.Models
{
    public class ProjectMember
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int MemberId { get; set; }
        public User Member { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}
