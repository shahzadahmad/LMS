namespace LMS.Application.DTOs
{
    public class QuestionDTO
    {
        public int QuestionId { get; set; }
        public int AssessmentId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
