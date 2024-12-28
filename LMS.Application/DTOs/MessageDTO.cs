namespace LMS.Application.DTOs
{
    public class MessageDTO
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public DateTime SentAt { get; set; }
    }
}
