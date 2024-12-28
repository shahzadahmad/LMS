using LMS.Domain.Entities;
using System.Linq.Expressions;

namespace LMS.Domain.IRepositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetAllAsync(Expression<Func<Message, bool>> predicate = null);
        Task<Message> GetByIdAsync(int id);
        Task AddAsync(Message message);        
        Task UpdateAsync(Message message);
        Task DeleteAsync(int id);
    }
}
