using DocLocker.Core.Models;

namespace DocLocker.Web.Models
{
    public class AdminProjectFormViewModel
    {
        public int? ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ManagerId { get; set; }
        public IReadOnlyList<UserSummaryDTO> Managers { get; set; } = Array.Empty<UserSummaryDTO>();
    }
}
