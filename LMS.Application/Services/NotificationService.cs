using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMS.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(INotificationRepository notificationRepository, IDistributedCache cache, ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a notification by its ID, checking the cache first.
        /// Logs detailed information about the cache retrieval process and any errors.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to retrieve.</param>
        /// <returns>A <see cref="NotificationDTO"/> representing the notification, or <c>null</c> if not found.</returns>
        public async Task<NotificationDTO> GetNotificationByIdAsync(int notificationId)
        {
            var cacheKey = $"Notification:{notificationId}";

            try
            {
                // Attempt to get notification from cache
                var cachedNotification = await _cache.GetStringAsync(cacheKey);
                if (cachedNotification != null)
                {
                    _logger.LogInformation("Cache hit for notification with ID {NotificationId}.", notificationId);
                    return JsonSerializer.Deserialize<NotificationDTO>(cachedNotification);
                }

                // Retrieve notification from repository
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    _logger.LogWarning("Notification with ID {NotificationId} not found.", notificationId);
                    return null;
                }

                // Convert to DTO and cache the result
                var notificationDto = new NotificationDTO
                {
                    NotificationId = notification.NotificationId,
                    UserId = notification.UserId,
                    Message = notification.Content,
                    IsRead = notification.IsRead,
                    DateCreated = notification.CreatedAt
                };

                var notificationJson = JsonSerializer.Serialize(notificationDto);
                await _cache.SetStringAsync(cacheKey, notificationJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

                _logger.LogInformation("Notification with ID {NotificationId} retrieved and cached.", notificationId);
                return notificationDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving notification with ID {NotificationId}.", notificationId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all notifications for a specific user, checking the cache first.
        /// Logs detailed information about the cache retrieval process and any errors.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve notifications for.</param>
        /// <returns>A list of <see cref="NotificationDTO"/> objects representing the user's notifications.</returns>
        public async Task<IEnumerable<NotificationDTO>> GetNotificationsByUserAsync(int userId)
        {
            var cacheKey = $"UserNotifications:{userId}";

            try
            {
                // Attempt to get notifications from cache
                var cachedNotifications = await _cache.GetStringAsync(cacheKey);
                if (cachedNotifications != null)
                {
                    _logger.LogInformation("Cache hit for notifications of user with ID {UserId}.", userId);
                    return JsonSerializer.Deserialize<IEnumerable<NotificationDTO>>(cachedNotifications);
                }

                // Retrieve notifications from repository
                var notifications = await _notificationRepository.GetAllAsync(n => n.UserId == userId);
                if (notifications == null || !notifications.Any())
                {
                    _logger.LogWarning("No notifications found for user with ID {UserId}.", userId);
                    return Enumerable.Empty<NotificationDTO>();
                }

                // Convert to DTOs and cache the result
                var notificationDtos = notifications.Select(notification => new NotificationDTO
                {
                    NotificationId = notification.NotificationId,
                    UserId = notification.UserId,
                    Message = notification.Content,
                    IsRead = notification.IsRead,
                    DateCreated = notification.CreatedAt
                }).ToList();

                var notificationsJson = JsonSerializer.Serialize(notificationDtos);
                await _cache.SetStringAsync(cacheKey, notificationsJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

                _logger.LogInformation("Notifications for user with ID {UserId} retrieved and cached.", userId);
                return notificationDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving notifications for user with ID {UserId}.", userId);
                throw;
            }
        }

        /// <summary>
        /// Marks a notification as read and updates it in the repository.
        /// Logs the process of marking the notification as read and any errors.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to mark as read.</param>
        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                // Retrieve the notification from repository
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    _logger.LogWarning("Notification with ID {NotificationId} not found for marking as read.", notificationId);
                    return;
                }

                // Mark as read and update repository
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);

                _logger.LogInformation("Notification with ID {NotificationId} marked as read.", notificationId);

                // Invalidate the cache
                var cacheKey = $"Notification:{notificationId}";
                await _cache.RemoveAsync(cacheKey);

                _logger.LogInformation("Cache entry for notification with ID {NotificationId} removed.", notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while marking notification with ID {NotificationId} as read.", notificationId);
                throw;
            }
        }

        /// <summary>
        /// Sends a new notification and adds it to the repository.
        /// Logs the process of sending the notification and any errors.
        /// </summary>
        /// <param name="notificationDto">The <see cref="NotificationDTO"/> object containing notification details.</param>
        public async Task SendNotificationAsync(NotificationDTO notificationDto)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = notificationDto.UserId,
                    Content = notificationDto.Message,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.AddAsync(notification);

                _logger.LogInformation("Notification sent to user with ID {UserId}.", notificationDto.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending notification to user with ID {UserId}.", notificationDto.UserId);
                throw;
            }
        }
    }
}
