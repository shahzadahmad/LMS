using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMS.Application.Services
{
    /// <summary>
    /// Provides methods for managing questions, including CRUD operations.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<QuestionService> _logger;

        public QuestionService(
            IQuestionRepository questionRepository,
            IDistributedCache cache,
            ILogger<QuestionService> logger)
        {
            _questionRepository = questionRepository;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a question by its ID from cache or repository.
        /// </summary>
        /// <param name="id">The ID of the question to retrieve.</param>
        /// <returns>A Task representing the asynchronous operation, with the question data as the result.</returns>
        public async Task<QuestionDTO> GetQuestionByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving question with ID {Id}", id);

            try
            {
                var cacheKey = $"Question_{id}";
                var cachedQuestion = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedQuestion))
                {
                    _logger.LogInformation("Question with ID {Id} retrieved from cache.", id);
                    return JsonSerializer.Deserialize<QuestionDTO>(cachedQuestion);
                }

                _logger.LogInformation("Question with ID {Id} not found in cache. Fetching from repository.", id);
                var question = await _questionRepository.GetByIdAsync(id);
                if (question == null)
                {
                    _logger.LogWarning("Question with ID {Id} not found in repository.", id);
                    return null;
                }

                var questionDto = MapToDTO(question);
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(questionDto), cacheOptions);

                _logger.LogInformation("Question with ID {Id} retrieved from repository and cached.", id);
                return questionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving question with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all questions from cache or repository.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation, with a list of all questions as the result.</returns>
        public async Task<IEnumerable<QuestionDTO>> GetAllQuestionsAsync()
        {
            _logger.LogInformation("Retrieving all questions");

            try
            {
                var cacheKey = "AllQuestions";
                var cachedQuestions = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedQuestions))
                {
                    _logger.LogInformation("All questions retrieved from cache.");
                    return JsonSerializer.Deserialize<IEnumerable<QuestionDTO>>(cachedQuestions);
                }

                _logger.LogInformation("Questions not found in cache. Fetching from repository.");
                var questions = await _questionRepository.GetAllAsync();
                if (questions == null || !questions.Any())
                {
                    _logger.LogInformation("No questions found in repository.");
                    return Enumerable.Empty<QuestionDTO>();
                }

                var questionsDto = MapToDTO(questions);
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(questionsDto), cacheOptions);

                _logger.LogInformation("All questions retrieved from repository and cached.");
                return questionsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all questions.");
                throw;
            }
        }

        /// <summary>
        /// Adds a new question to the repository.
        /// </summary>
        /// <param name="createQuestionDto">The data transfer object containing the information for the new question.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(CreateQuestionDTO createQuestionDto)
        {
            _logger.LogInformation("Adding new question with Assessment ID {AssessmentId}", createQuestionDto.AssessmentId);

            try
            {
                var question = new Question
                {
                    AssessmentId = createQuestionDto.AssessmentId,
                    Content = createQuestionDto.Content,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _questionRepository.AddAsync(question);
                _logger.LogInformation("Successfully added question with ID {QuestionId}", question.QuestionId);

                // Clear the cache for questions list
                await _cache.RemoveAsync("Questions");
                _logger.LogInformation("Cache cleared for questions list after adding new question with ID {QuestionId}", question.QuestionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding question with Assessment ID {AssessmentId}", createQuestionDto.AssessmentId);
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        /// <summary>
        /// Updates an existing question in the repository.
        /// </summary>
        /// <param name="updateQuestionDto">The data transfer object containing the updated information for the question.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(UpdateQuestionDTO updateQuestionDto)
        {
            _logger.LogInformation("Updating question with ID {QuestionId}", updateQuestionDto.QuestionId);

            try
            {
                var question = await _questionRepository.GetByIdAsync(updateQuestionDto.QuestionId);
                if (question == null)
                {
                    _logger.LogWarning("Question with ID {QuestionId} not found", updateQuestionDto.QuestionId);
                    return; // Optionally handle if the question is not found
                }

                question.Content = updateQuestionDto.Content;
                question.UpdatedAt = DateTime.UtcNow;

                await _questionRepository.UpdateAsync(question);
                _logger.LogInformation("Successfully updated question with ID {QuestionId}", updateQuestionDto.QuestionId);

                // Clear the cache for the specific question and questions list
                await _cache.RemoveAsync($"Question_{updateQuestionDto.QuestionId}");
                await _cache.RemoveAsync("Questions");
                _logger.LogInformation("Cache cleared for question with ID {QuestionId} and questions list after update", updateQuestionDto.QuestionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating question with ID {QuestionId}", updateQuestionDto.QuestionId);
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        /// <summary>
        /// Deletes a question by its ID from the repository.
        /// </summary>
        /// <param name="id">The ID of the question to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting question with ID {QuestionId}", id);

            try
            {
                await _questionRepository.DeleteAsync(id);
                _logger.LogInformation("Successfully deleted question with ID {QuestionId}", id);

                // Clear the cache for the specific question and questions list
                await _cache.RemoveAsync($"Question_{id}");
                await _cache.RemoveAsync("Questions");
                _logger.LogInformation("Cache cleared for question with ID {QuestionId} and questions list after deletion", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting question with ID {QuestionId}", id);
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        /// <summary>
        /// Maps a Question entity to a QuestionDTO.
        /// </summary>
        /// <param name="question">The Question entity to map.</param>
        /// <returns>A QuestionDTO representing the provided Question entity.</returns>
        private QuestionDTO MapToDTO(Question question)
        {
            if (question == null)
            {
                throw new ArgumentNullException(nameof(question), "The question entity cannot be null.");
            }

            return new QuestionDTO
            {
                QuestionId = question.QuestionId,
                AssessmentId = question.AssessmentId,
                Content = question.Content,
                CreatedAt = question.CreatedAt,
                UpdatedAt = question.UpdatedAt
            };
        }

        /// <summary>
        /// Maps a collection of Question entities to a collection of QuestionDTOs.
        /// </summary>
        /// <param name="questions">The collection of Question entities to map.</param>
        /// <returns>A collection of QuestionDTOs representing the provided Question entities.</returns>
        private IEnumerable<QuestionDTO> MapToDTO(IEnumerable<Question> questions)
        {
            if (questions == null)
            {
                throw new ArgumentNullException(nameof(questions), "The collection of questions cannot be null.");
            }

            // Map each Question entity in the collection to a QuestionDTO
            return questions.Select(question => MapToDTO(question)).ToList();
        }
    }
}
