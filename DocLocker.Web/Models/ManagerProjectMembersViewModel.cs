using DocLocker.Core.Models;

namespace DocLocker.Web.Models
{
    public class ManagerProjectMembersViewModel
    {
        public ProjectSummaryDTO Project { get; set; } = new ProjectSummaryDTO();
        public IReadOnlyList<UserSummaryDTO> AssignedMembers { get; set; } = Array.Empty<UserSummaryDTO>();
        public IReadOnlyList<UserSummaryDTO> AvailableMembers { get; set; } = Array.Empty<UserSummaryDTO>();
    }
}
