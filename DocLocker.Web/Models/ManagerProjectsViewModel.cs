namespace DocLocker.Web.Models
{
    public class ManagerProjectsViewModel
    {
        public List<ManagerProjectMembersViewModel> Projects { get; set; } = new();
        public int? ActiveRequestProjectId { get; set; }
        public DocLocker.Core.Models.CreateDocumentRequestDTO? RequestForm { get; set; }
    }
}
