using System.ComponentModel.DataAnnotations;

namespace DocLocker.Core.Models
{
    public class AssignProjectMemberDTO
    {
        [Required]
        public int MemberId { get; set; }
    }
}
