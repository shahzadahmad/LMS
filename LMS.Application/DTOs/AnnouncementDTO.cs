namespace LMS.Application.DTOs
{
    public class AnnouncementDTO
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? CourseId { get; set; }
    }
}
