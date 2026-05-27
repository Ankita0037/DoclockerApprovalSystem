using DocLocker.Core.Models;

namespace DocLocker.Web.Models
{
    public class AdminProjectsViewModel
    {
        public IReadOnlyList<ProjectSummaryDTO> Projects { get; set; } = Array.Empty<ProjectSummaryDTO>();
        public IReadOnlyList<UserSummaryDTO> Managers { get; set; } = Array.Empty<UserSummaryDTO>();
    }
}
