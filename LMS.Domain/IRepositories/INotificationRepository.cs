using LMS.Domain.Entities;
using System.Linq.Expressions;

namespace LMS.Domain.IRepositories
{
    public interface INotificationRepository
    {
        /// <summary>
        /// Retrieves a notification by its ID.
        /// </summary>
        /// <param name="id">The ID of the notification to retrieve.</param>
        /// <returns>A <see cref="Notification"/> representing the notification, or <c>null</c> if not found.</returns>
        Task<Notification> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all notifications that match the specified criteria.
        /// </summary>
        /// <param name="predicate">The criteria to filter notifications.</param>
        /// <returns>A list of <see cref="Notification"/> objects matching the criteria.</returns>
        Task<IEnumerable<Notification>> GetAllAsync(Expression<Func<Notification, bool>> predicate);

        /// <summary>
        /// Adds a new notification to the repository.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to add.</param>
        Task AddAsync(Notification notification);

        /// <summary>
        /// Updates an existing notification in the repository.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to update.</param>
        Task UpdateAsync(Notification notification);

        /// <summary>
        /// Deletes a notification from the repository by its ID.
        /// </summary>
        /// <param name="id">The ID of the notification to delete.</param>
        Task DeleteAsync(int id);
    }
}
