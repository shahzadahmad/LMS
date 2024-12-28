using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMS.Application.Services
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ILogger<LessonService> _logger;
        private readonly IDistributedCache _cache;

        private const string CachePrefix = "Lesson_";

        public LessonService(ILessonRepository lessonRepository, ILogger<LessonService> logger, IDistributedCache cache)
        {
            _lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Retrieves all lessons, either from cache or from the database.
        /// </summary>
        /// <returns>A list of LessonDTO objects.</returns>
        public async Task<IEnumerable<LessonDTO>> GetAllLessonsAsync()
        {
            _logger.LogInformation("Initiating request to fetch all lessons.");

            try
            {
                // Check if lessons are cached
                var cachedLessons = await _cache.GetStringAsync($"{CachePrefix}All");
                if (!string.IsNullOrEmpty(cachedLessons))
                {
                    _logger.LogInformation("Lessons found in cache.");
                    return JsonSerializer.Deserialize<IEnumerable<LessonDTO>>(cachedLessons);
                }

                _logger.LogInformation("Lessons not found in cache. Fetching from database.");
                var lessons = await _lessonRepository.GetAllAsync();

                if (lessons == null || !lessons.Any())
                {
                    _logger.LogInformation("No lessons found in the database.");
                    return Enumerable.Empty<LessonDTO>();
                }

                var lessonDtos = lessons.Select(lesson => new LessonDTO
                {
                    LessonId = lesson.LessonId,
                    LessonName = lesson.Title,
                    Content = lesson.Content,
                    ModuleId = lesson.ModuleId
                }).ToList();

                // Cache the lessons
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                await _cache.SetStringAsync($"{CachePrefix}All", JsonSerializer.Serialize(lessonDtos), cacheOptions);

                _logger.LogInformation("Lessons fetched from database and cached.");
                return lessonDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all lessons.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific lesson by its ID, either from cache or from the database.
        /// </summary>
        /// <param name="lessonId">The ID of the lesson to retrieve.</param>
        /// <returns>A LessonDTO object, or null if not found.</returns>
        public async Task<LessonDTO> GetLessonByIdAsync(int lessonId)
        {
            _logger.LogInformation("Initiating request to fetch lesson with ID: {LessonId}.", lessonId);

            try
            {
                // Check if lesson is cached
                var cachedLesson = await _cache.GetStringAsync($"{CachePrefix}{lessonId}");
                if (!string.IsNullOrEmpty(cachedLesson))
                {
                    _logger.LogInformation("Lesson with ID: {LessonId} found in cache.", lessonId);
                    return JsonSerializer.Deserialize<LessonDTO>(cachedLesson);
                }

                _logger.LogInformation("Lesson with ID: {LessonId} not found in cache. Fetching from database.", lessonId);
                var lesson = await _lessonRepository.GetByIdAsync(lessonId);

                if (lesson == null)
                {
                    _logger.LogWarning("Lesson with ID: {LessonId} not found in the database.", lessonId);
                    return null;
                }

                var lessonDto = new LessonDTO
                {
                    LessonId = lesson.LessonId,
                    LessonName = lesson.Title,
                    Content = lesson.Content,
                    ModuleId = lesson.ModuleId
                };

                // Cache the lesson
                await _cache.SetStringAsync($"{CachePrefix}{lessonId}", JsonSerializer.Serialize(lessonDto));

                _logger.LogInformation("Lesson with ID: {LessonId} fetched from database and cached.", lessonId);
                return lessonDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching lesson with ID: {LessonId}.", lessonId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new lesson and adds it to the database.
        /// </summary>
        /// <param name="createLessonDto">The DTO containing lesson details.</param>
        /// <returns>The ID of the newly created lesson.</returns>
        public async Task<int> CreateLessonAsync(CreateLessonDTO createLessonDto)
        {
            _logger.LogInformation("Initiating request to create a new lesson.");

            try
            {
                var lesson = new Lesson
                {
                    Title = createLessonDto.LessonName,
                    Content = createLessonDto.Content,
                    ModuleId = createLessonDto.ModuleId
                };

                await _lessonRepository.AddAsync(lesson);

                _logger.LogInformation("Lesson created with ID: {LessonId}.", lesson.LessonId);

                // Invalidate cache for all lessons
                await _cache.RemoveAsync($"{CachePrefix}All");

                return lesson.LessonId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new lesson.");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing lesson in the database.
        /// </summary>
        /// <param name="lessonId">The ID of the lesson to update.</param>
        /// <param name="updateLessonDto">The DTO containing updated lesson details.</param>
        public async Task UpdateLessonAsync(int lessonId, UpdateLessonDTO updateLessonDto)
        {
            _logger.LogInformation("Initiating request to update lesson with ID: {LessonId}.", lessonId);

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(lessonId);
                if (lesson == null)
                {
                    _logger.LogWarning("Lesson with ID: {LessonId} not found.", lessonId);
                    return;
                }

                lesson.Title = updateLessonDto.LessonName;
                lesson.Content = updateLessonDto.Content;
                lesson.UpdatedAt = DateTime.UtcNow;

                await _lessonRepository.UpdateAsync(lesson);

                _logger.LogInformation("Lesson with ID: {LessonId} updated.", lessonId);

                // Update cache for the updated lesson
                await _cache.SetStringAsync($"{CachePrefix}{lessonId}", JsonSerializer.Serialize(new LessonDTO
                {
                    LessonId = lesson.LessonId,
                    LessonName = lesson.Title,
                    Content = lesson.Content,
                    ModuleId = lesson.ModuleId
                }));

                // Invalidate cache for all lessons
                await _cache.RemoveAsync($"{CachePrefix}All");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating lesson with ID: {LessonId}.", lessonId);
                throw;
            }
        }

        /// <summary>
        /// Deletes a lesson from the database.
        /// </summary>
        /// <param name="lessonId">The ID of the lesson to delete.</param>
        public async Task DeleteLessonAsync(int lessonId)
        {
            _logger.LogInformation("Initiating request to delete lesson with ID: {LessonId}.", lessonId);

            try
            {
                await _lessonRepository.DeleteAsync(lessonId);

                _logger.LogInformation("Lesson with ID: {LessonId} deleted.", lessonId);

                // Remove the deleted lesson from cache
                await _cache.RemoveAsync($"{CachePrefix}{lessonId}");

                // Invalidate cache for all lessons
                await _cache.RemoveAsync($"{CachePrefix}All");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting lesson with ID: {LessonId}.", lessonId);
                throw;
            }
        }
    }
}
