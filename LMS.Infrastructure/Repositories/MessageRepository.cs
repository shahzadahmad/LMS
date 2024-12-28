using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LMS.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly LMSDbContext _context;
        private readonly DbSet<Message> _messages;

        public MessageRepository(LMSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _messages = _context.Set<Message>();
        }

        /// <summary>
        /// Retrieves all messages that match the given filter predicate.
        /// </summary>
        /// <param name="predicate">The filter predicate to apply.</param>
        /// <returns>A list of <see cref="Message"/> entities.</returns>
        public async Task<IEnumerable<Message>> GetAllAsync(Expression<Func<Message, bool>> predicate = null)
        {
            var query = _messages.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync();
        }


        /// <summary>
        /// Retrieves a message by its ID.
        /// </summary>
        /// <param name="id">The ID of the message to retrieve.</param>
        /// <returns>The <see cref="Message"/> entity if found; otherwise, <c>null</c>.</returns>
        public async Task<Message> GetByIdAsync(int id)
        {
            return await _messages.FindAsync(id);
        }

        /// <summary>
        /// Adds a new message to the repository.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> entity to add.</param>
        public async Task AddAsync(Message message)
        {
            await _messages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing message in the repository.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> entity to update.</param>
        public async Task UpdateAsync(Message message)
        {
            _messages.Update(message);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a message by its ID.
        /// </summary>
        /// <param name="id">The ID of the message to delete.</param>
        public async Task DeleteAsync(int id)
        {
            var message = await _messages.FindAsync(id);
            if (message != null)
            {
                _messages.Remove(message);
                await _context.SaveChangesAsync();
            }
        }
    }
}
