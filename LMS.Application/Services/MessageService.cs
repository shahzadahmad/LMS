using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMS.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IMessageRepository messageRepository, IDistributedCache cache, ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a message by its ID, checking the cache first.
        /// Logs the outcome of cache operations and any exceptions encountered.
        /// </summary>
        /// <param name="messageId">The ID of the message to retrieve.</param>
        /// <returns>A <see cref="MessageDTO"/> representing the message, or <c>null</c> if not found.</returns>
        public async Task<MessageDTO> GetMessageByIdAsync(int messageId)
        {
            _logger.LogInformation("Retrieving message with ID {MessageId}.", messageId);

            try
            {
                var cacheKey = $"Message:{messageId}";
                var cachedMessage = await _cache.GetStringAsync(cacheKey);

                if (cachedMessage != null)
                {
                    _logger.LogInformation("Cache hit for message with ID {MessageId}.", messageId);
                    return JsonSerializer.Deserialize<MessageDTO>(cachedMessage);
                }

                _logger.LogInformation("Cache miss for message with ID {MessageId}. Fetching from repository.", messageId);

                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null)
                {
                    _logger.LogWarning("Message with ID {MessageId} not found in repository.", messageId);
                    return null;
                }

                var messageDto = new MessageDTO
                {
                    MessageId = message.MessageId,
                    SenderId = message.SenderId,
                    RecipientId = message.ReceiverId,
                    Content = message.Content,
                    SentAt = message.SentAt
                };

                var messageJson = JsonSerializer.Serialize(messageDto);
                await _cache.SetStringAsync(cacheKey, messageJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

                _logger.LogInformation("Message with ID {MessageId} retrieved from repository and cached.", messageId);
                return messageDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving message with ID {MessageId}.", messageId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all messages for a specific user, either sent or received, checking the cache first.
        /// Logs the outcome of cache operations and any exceptions encountered.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve messages for.</param>
        /// <returns>A list of <see cref="MessageDTO"/> objects representing the user's messages.</returns>
        public async Task<IEnumerable<MessageDTO>> GetMessagesByUserAsync(int userId)
        {
            _logger.LogInformation("Retrieving messages for user with ID {UserId}.", userId);

            try
            {
                var cacheKey = $"UserMessages:{userId}";
                var cachedMessages = await _cache.GetStringAsync(cacheKey);

                if (cachedMessages != null)
                {
                    _logger.LogInformation("Cache hit for messages of user with ID {UserId}.", userId);
                    return JsonSerializer.Deserialize<IEnumerable<MessageDTO>>(cachedMessages);
                }

                _logger.LogInformation("Cache miss for messages of user with ID {UserId}. Fetching from repository.", userId);

                var messages = await _messageRepository.GetAllAsync(m => m.SenderId == userId || m.ReceiverId == userId);
                if (messages == null || !messages.Any())
                {
                    _logger.LogWarning("No messages found for user with ID {UserId}.", userId);
                    return Enumerable.Empty<MessageDTO>();
                }

                var messageDtos = messages.Select(message => new MessageDTO
                {
                    MessageId = message.MessageId,
                    SenderId = message.SenderId,
                    RecipientId = message.ReceiverId,
                    Content = message.Content,
                    SentAt = message.SentAt
                }).ToList();

                var messagesJson = JsonSerializer.Serialize(messageDtos);
                await _cache.SetStringAsync(cacheKey, messagesJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

                _logger.LogInformation("Messages for user with ID {UserId} retrieved from repository and cached.", userId);
                return messageDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving messages for user with ID {UserId}.", userId);
                throw;
            }
        }

        /// <summary>
        /// Sends a new message and adds it to the repository.
        /// Logs the outcome of the operation and any exceptions encountered.
        /// </summary>
        /// <param name="messageDto">The <see cref="MessageDTO"/> object containing message details.</param>
        public async Task SendMessageAsync(MessageDTO messageDto)
        {
            _logger.LogInformation("Sending message from user {SenderId} to user {RecipientId}.", messageDto.SenderId, messageDto.RecipientId);

            try
            {
                var message = new Message
                {
                    SenderId = messageDto.SenderId,
                    ReceiverId = messageDto.RecipientId,
                    Content = messageDto.Content,
                    SentAt = DateTime.UtcNow
                };

                await _messageRepository.AddAsync(message);

                _logger.LogInformation("Message successfully sent from user {SenderId} to user {RecipientId}.", messageDto.SenderId, messageDto.RecipientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending a message from user {SenderId} to user {RecipientId}.", messageDto.SenderId, messageDto.RecipientId);
                throw;
            }
        }
    }
}
