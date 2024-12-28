using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Apply global authorization
    [EnableRateLimiting("GlobalRateLimitPolicy")] // Apply the rate limiting policy to the entire controller
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a notification by its ID.
        /// Logs the process of retrieving the notification and any errors.
        /// </summary>
        /// <param name="id">The ID of the notification to retrieve.</param>
        /// <returns>A response containing the notification details.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Instructor,Student")] // Example of role-based authorization
        public async Task<IActionResult> GetNotificationById(int id)
        {
            _logger.LogInformation("Request to retrieve notification with ID {NotificationId}.", id);

            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    _logger.LogWarning("Notification with ID {NotificationId} not found.", id);
                    return NotFound();
                }

                _logger.LogInformation("Notification with ID {NotificationId} successfully retrieved.", id);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving notification with ID {NotificationId}.", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Retrieves all notifications for a specific user.
        /// Logs the process of retrieving notifications and any errors.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve notifications for.</param>
        /// <returns>A response containing a list of notifications for the user.</returns>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> GetNotificationsByUser(int userId)
        {
            _logger.LogInformation("Request to retrieve notifications for user with ID {UserId}.", userId);

            try
            {
                var notifications = await _notificationService.GetNotificationsByUserAsync(userId);
                if (notifications == null || !notifications.Any())
                {
                    _logger.LogWarning("No notifications found for user with ID {UserId}.", userId);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved notifications for user with ID {UserId}.", userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving notifications for user with ID {UserId}.", userId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Marks a notification as read.
        /// Logs the process of marking the notification as read and any errors.
        /// </summary>
        /// <param name="id">The ID of the notification to mark as read.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpPost("mark-read/{id}")]
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            _logger.LogInformation("Request to mark notification with ID {NotificationId} as read.", id);

            try
            {
                await _notificationService.MarkNotificationAsReadAsync(id);
                _logger.LogInformation("Notification with ID {NotificationId} successfully marked as read.", id);
                return NoContent(); // HTTP 204 No Content
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while marking notification with ID {NotificationId} as read.", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Sends a new notification.
        /// Logs the process of sending the notification and any errors.
        /// </summary>
        /// <param name="notificationDto">The <see cref="NotificationDTO"/> object containing notification details.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDTO notificationDto)
        {
            _logger.LogInformation("Request to send a new notification for user {UserId}.", notificationDto.UserId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for notification creation.");
                return BadRequest(ModelState);
            }

            try
            {
                await _notificationService.SendNotificationAsync(notificationDto);
                _logger.LogInformation("Notification successfully sent to user {UserId}.", notificationDto.UserId);
                return CreatedAtAction(nameof(GetNotificationById), new { id = notificationDto.NotificationId }, notificationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending a notification to user {UserId}.", notificationDto.UserId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
