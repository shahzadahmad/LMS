namespace LMS.Application.DTOs
{
    public class DiscussionForumDTO
    {
        public int ForumId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? CourseId { get; set; }
    }
}
