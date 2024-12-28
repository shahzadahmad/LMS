namespace LMS.Application.DTOs
{
    public class UpdateNotificationDTO
    {
        public string Message { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsRead { get; set; }
    }
}
