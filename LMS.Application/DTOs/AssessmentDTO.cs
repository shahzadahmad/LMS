namespace LMS.Application.DTOs
{
    public class AssessmentDTO
    {
        public int AssessmentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CourseId { get; set; }
    }
}
