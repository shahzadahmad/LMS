using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LMS.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly LMSDbContext _context;

        public NotificationRepository(LMSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves a notification by its ID.
        /// </summary>
        /// <param name="id">The ID of the notification to retrieve.</param>
        /// <returns>A <see cref="Notification"/> representing the notification, or <c>null</c> if not found.</returns>
        public async Task<Notification> GetByIdAsync(int id)
        {
            return await _context.Notifications
                .Include(n => n.User) // Include related User entity if needed
                .FirstOrDefaultAsync(n => n.NotificationId == id);
        }

        /// <summary>
        /// Retrieves all notifications that match the specified criteria.
        /// </summary>
        /// <param name="predicate">The criteria to filter notifications.</param>
        /// <returns>A list of <see cref="Notification"/> objects matching the criteria.</returns>
        public async Task<IEnumerable<Notification>> GetAllAsync(Expression<Func<Notification, bool>> predicate)
        {
            return await _context.Notifications
                .Include(n => n.User) // Include related User entity if needed
                .Where(predicate)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new notification to the repository.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to add.</param>
        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing notification in the repository.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to update.</param>
        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a notification from the repository by its ID.
        /// </summary>
        /// <param name="id">The ID of the notification to delete.</param>
        public async Task DeleteAsync(int id)
        {
            var notification = await GetByIdAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }
    }
}
