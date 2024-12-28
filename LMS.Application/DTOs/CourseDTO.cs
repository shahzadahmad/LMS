using System.Collections.Generic;

namespace LMS.Application.DTOs
{
    public class CourseDTO
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public List<ModuleDTO> Modules { get; set; }
    }
}
