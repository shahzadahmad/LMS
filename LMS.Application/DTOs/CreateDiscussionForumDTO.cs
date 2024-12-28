namespace LMS.Application.DTOs
{
    public class CreateDiscussionForumDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public int? CourseId { get; set; }
    }
}
