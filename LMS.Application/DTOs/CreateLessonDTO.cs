namespace LMS.Application.DTOs
{
    public class CreateLessonDTO
    {
        public string LessonName { get; set; }
        public string Content { get; set; }
        public int ModuleId { get; set; }
    }
}
