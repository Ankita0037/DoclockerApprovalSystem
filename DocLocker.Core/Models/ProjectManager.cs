namespace DocLocker.Core.Models
{
    public class ProjectManager
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int ManagerId { get; set; }
        public User Manager { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;
    }
}
