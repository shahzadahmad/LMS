using LMS.Application.DTOs;

namespace LMS.Application.Interfaces
{
    public interface IMessageService
    {
        Task<MessageDTO> GetMessageByIdAsync(int messageId);
        Task<IEnumerable<MessageDTO>> GetMessagesByUserAsync(int userId);
        Task SendMessageAsync(MessageDTO messageDto);
    }
}
