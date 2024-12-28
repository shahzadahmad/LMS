namespace LMS.Application.DTOs
{
    public class CreateModuleDTO
    {        
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public int CourseId { get; set; }
    }
}
