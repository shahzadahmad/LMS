using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LMS.Application.Services
{
    public class AnswerService : IAnswerService
    {
        private readonly IAnswerRepository _answerRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<AnswerService> _logger;

        private const string CacheKeyPrefix = "Answer_";

        public AnswerService(IAnswerRepository answerRepository, IDistributedCache cache, ILogger<AnswerService> logger)
        {
            _answerRepository = answerRepository ?? throw new ArgumentNullException(nameof(answerRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all answers from the repository, with caching.
        /// </summary>
        /// <returns>A collection of AnswerDTO objects.</returns>
        public async Task<IEnumerable<AnswerDTO>> GetAllAsync()
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}All";
                var cachedAnswers = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedAnswers))
                {
                    _logger.LogInformation("Returning answers from cache.");
                    return JsonConvert.DeserializeObject<IEnumerable<AnswerDTO>>(cachedAnswers);
                }

                var answers = await _answerRepository.GetAllAsync();
                var answerDTOs = answers.Select(ConvertToDTO).ToList();

                var serializedAnswers = JsonConvert.SerializeObject(answerDTOs);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };

                await _cache.SetStringAsync(cacheKey, serializedAnswers, options);

                _logger.LogInformation("Returning answers from repository and caching.");
                return answerDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all answers.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific answer by its ID, with caching.
        /// </summary>
        /// <param name="id">The ID of the answer.</param>
        /// <returns>An AnswerDTO object.</returns>
        public async Task<AnswerDTO> GetByIdAsync(int id)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}{id}";
                var cachedAnswer = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedAnswer))
                {
                    _logger.LogInformation("Returning answer with ID {AnswerId} from cache.", id);
                    return JsonConvert.DeserializeObject<AnswerDTO>(cachedAnswer);
                }

                var answer = await _answerRepository.GetByIdAsync(id);
                if (answer == null)
                {
                    _logger.LogWarning("Answer with ID {AnswerId} not found.", id);
                    return null;
                }

                var answerDTO = ConvertToDTO(answer);
                var serializedAnswer = JsonConvert.SerializeObject(answerDTO);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };

                await _cache.SetStringAsync(cacheKey, serializedAnswer, options);

                _logger.LogInformation("Returning answer with ID {AnswerId} from repository and caching.", id);
                return answerDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving answer with ID {AnswerId}.", id);
                throw;
            }
        }

        /// <summary>
        /// Adds a new answer and clears cache.
        /// </summary>
        /// <param name="createAnswerDTO">The DTO containing answer data.</param>
        public async Task AddAsync(CreateAnswerDTO createAnswerDTO)
        {
            try
            {
                if (createAnswerDTO == null)
                {
                    _logger.LogWarning("Received null CreateAnswerDTO.");
                    throw new ArgumentNullException(nameof(createAnswerDTO));
                }

                var answer = new Answer
                {
                    QuestionId = createAnswerDTO.QuestionId,
                    Content = createAnswerDTO.Content,
                    IsCorrect = createAnswerDTO.IsCorrect,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _answerRepository.AddAsync(answer);
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");

                _logger.LogInformation("Added new answer with QuestionId {QuestionId}.", createAnswerDTO.QuestionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new answer.");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing answer and clears cache.
        /// </summary>
        /// <param name="updateAnswerDTO">The DTO containing updated answer data.</param>
        public async Task UpdateAsync(UpdateAnswerDTO updateAnswerDTO)
        {
            try
            {
                if (updateAnswerDTO == null)
                {
                    _logger.LogWarning("Received null UpdateAnswerDTO.");
                    throw new ArgumentNullException(nameof(updateAnswerDTO));
                }

                var existingAnswer = await _answerRepository.GetByIdAsync(updateAnswerDTO.AnswerId);
                if (existingAnswer == null)
                {
                    _logger.LogWarning("Answer with ID {AnswerId} not found.", updateAnswerDTO.AnswerId);
                    return;
                }

                existingAnswer.QuestionId = updateAnswerDTO.QuestionId;
                existingAnswer.Content = updateAnswerDTO.Content;
                existingAnswer.IsCorrect = updateAnswerDTO.IsCorrect;
                existingAnswer.UpdatedAt = DateTime.UtcNow;

                await _answerRepository.UpdateAsync(existingAnswer);
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");
                await _cache.RemoveAsync($"{CacheKeyPrefix}{updateAnswerDTO.AnswerId}");

                _logger.LogInformation("Updated answer with ID {AnswerId}.", updateAnswerDTO.AnswerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating answer with ID {AnswerId}.", updateAnswerDTO.AnswerId);
                throw;
            }
        }

        /// <summary>
        /// Deletes an answer and clears cache.
        /// </summary>
        /// <param name="id">The ID of the answer to be deleted.</param>
        public async Task DeleteAsync(int id)
        {
            try
            {
                var existingAnswer = await _answerRepository.GetByIdAsync(id);
                if (existingAnswer == null)
                {
                    _logger.LogWarning("Answer with ID {AnswerId} not found.", id);
                    return;
                }

                await _answerRepository.DeleteAsync(id);
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");
                await _cache.RemoveAsync($"{CacheKeyPrefix}{id}");

                _logger.LogInformation("Deleted answer with ID {AnswerId}.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting answer with ID {AnswerId}.", id);
                throw;
            }
        }

        /// <summary>
        /// Converts an Answer entity to an AnswerDTO.
        /// </summary>
        /// <param name="answer">The Answer entity to convert.</param>
        /// <returns>The corresponding AnswerDTO.</returns>
        private AnswerDTO ConvertToDTO(Answer answer)
        {
            return new AnswerDTO
            {
                AnswerId = answer.AnswerId,
                QuestionId = answer.QuestionId,
                Content = answer.Content,
                IsCorrect = answer.IsCorrect,
                CreatedAt = answer.CreatedAt,
                UpdatedAt = answer.UpdatedAt
            };
        }
    }
}
