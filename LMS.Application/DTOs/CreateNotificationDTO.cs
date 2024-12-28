namespace LMS.Application.DTOs
{
    public class CreateNotificationDTO
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsRead { get; set; }
    }
}
