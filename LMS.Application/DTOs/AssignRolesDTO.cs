using System.ComponentModel.DataAnnotations;

namespace LMS.Application.DTOs
{
    public class AssignRolesDTO
    {
        [Required]
        public List<int> RoleIds { get; set; }
    }
}
