using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Apply global authorization
    [EnableRateLimiting("GlobalRateLimitPolicy")] // Apply the rate limiting policy to the entire controller
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a message by its ID.
        /// Logs the request, cache hits, and any errors encountered.
        /// Requires authorization for Admin, Instructor, or Student roles.
        /// </summary>
        /// <param name="messageId">The ID of the message to retrieve.</param>
        /// <returns>A <see cref="ActionResult{MessageDTO}"/> containing the message or a 404 if not found.</returns>
        [HttpGet("{messageId}")]
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<ActionResult<MessageDTO>> GetMessageByIdAsync(int messageId)
        {
            _logger.LogInformation("Received request to retrieve message with ID {MessageId}.", messageId);

            try
            {
                var message = await _messageService.GetMessageByIdAsync(messageId);

                if (message == null)
                {
                    _logger.LogWarning("Message with ID {MessageId} not found.", messageId);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved message with ID {MessageId}.", messageId);
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving message with ID {MessageId}.", messageId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves all messages for a specific user.
        /// Logs the request and any errors encountered.
        /// Requires authorization for Admin or Instructor roles.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve messages for.</param>
        /// <returns>A <see cref="ActionResult{IEnumerable{MessageDTO}}"/> containing the list of messages or a 404 if none found.</returns>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesByUserAsync(int userId)
        {
            _logger.LogInformation("Received request to retrieve messages for user with ID {UserId}.", userId);

            try
            {
                var messages = await _messageService.GetMessagesByUserAsync(userId);

                if (messages == null || !messages.Any())
                {
                    _logger.LogWarning("No messages found for user with ID {UserId}.", userId);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved messages for user with ID {UserId}.", userId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving messages for user with ID {UserId}.", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Sends a new message.
        /// Logs the request, the result of the send operation, and any errors encountered.
        /// Requires authorization for Admin, Instructor, or Student roles.
        /// </summary>
        /// <param name="messageDto">The <see cref="MessageDTO"/> object containing message details.</param>
        /// <returns>A <see cref="ActionResult"/> indicating the result of the operation.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<ActionResult> SendMessageAsync([FromBody] MessageDTO messageDto)
        {
            if (messageDto == null)
            {
                _logger.LogWarning("Received null MessageDTO object.");
                return BadRequest("MessageDTO object is null");
            }

            _logger.LogInformation("Received request to send a message from user {SenderId} to user {RecipientId}.", messageDto.SenderId, messageDto.RecipientId);

            try
            {
                await _messageService.SendMessageAsync(messageDto);
                _logger.LogInformation("Message successfully sent from user {SenderId} to user {RecipientId}.", messageDto.SenderId, messageDto.RecipientId);
                return Ok("Message sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending a message from user {SenderId} to user {RecipientId}.", messageDto.SenderId, messageDto.RecipientId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
