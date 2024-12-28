namespace LMS.Application.DTOs
{
    public class ModuleDTO
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public List<LessonDTO> Lessons { get; set; }
    }
}
