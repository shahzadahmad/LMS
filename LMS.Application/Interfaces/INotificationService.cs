using LMS.Application.DTOs;

namespace LMS.Application.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// Retrieves a notification by its ID.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to retrieve.</param>
        /// <returns>A <see cref="NotificationDTO"/> representing the notification, or <c>null</c> if not found.</returns>
        Task<NotificationDTO> GetNotificationByIdAsync(int notificationId);

        /// <summary>
        /// Retrieves all notifications for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve notifications for.</param>
        /// <returns>A list of <see cref="NotificationDTO"/> objects representing the user's notifications.</returns>
        Task<IEnumerable<NotificationDTO>> GetNotificationsByUserAsync(int userId);

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to mark as read.</param>
        Task MarkNotificationAsReadAsync(int notificationId);

        /// <summary>
        /// Sends a new notification.
        /// </summary>
        /// <param name="notificationDto">The <see cref="NotificationDTO"/> object containing notification details.</param>
        Task SendNotificationAsync(NotificationDTO notificationDto);
    }
}
