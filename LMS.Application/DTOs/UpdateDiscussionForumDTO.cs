namespace LMS.Application.DTOs
{
    public class UpdateDiscussionForumDTO
    {
        public int ForumId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? CourseId { get; set; }
    }
}
