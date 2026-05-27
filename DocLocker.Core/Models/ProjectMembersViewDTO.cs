namespace DocLocker.Core.Models
{
    public class ProjectMembersViewDTO
    {
        public IReadOnlyList<UserSummaryDTO> AssignedMembers { get; set; } = Array.Empty<UserSummaryDTO>();
        public IReadOnlyList<UserSummaryDTO> AvailableMembers { get; set; } = Array.Empty<UserSummaryDTO>();
    }
}
