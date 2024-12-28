using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMS.Application.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly ILogger<AssessmentService> _logger;
        private readonly IDistributedCache _cache;

        private const string CacheKeyPrefix = "Assessment_";

        public AssessmentService(IAssessmentRepository assessmentRepository, ILogger<AssessmentService> logger, IDistributedCache cache)
        {
            _assessmentRepository = assessmentRepository;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Retrieves all assessments.
        /// </summary>
        /// <returns>A collection of AssessmentDTO objects.</returns>
        public async Task<IEnumerable<AssessmentDTO>> GetAllAssessmentsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all assessments from repository");

                // Redis cache key for all assessments
                var cacheKey = $"{CacheKeyPrefix}All";

                // Try to get the cached value
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Returning cached assessments data");
                    return JsonSerializer.Deserialize<IEnumerable<AssessmentDTO>>(cachedData);
                }

                _logger.LogInformation("Assessments not found in cache, fetching from database");

                // Fetch assessments from repository
                var assessments = await _assessmentRepository.GetAllAsync();
                if (assessments == null || !assessments.Any())
                {
                    _logger.LogWarning("No assessments found in the database.");
                    return Enumerable.Empty<AssessmentDTO>();
                }

                _logger.LogInformation("Successfully fetched {AssessmentCount} assessments from database", assessments.Count());

                // Convert to DTOs
                var assessmentsDtos = assessments.Select(assessment => new AssessmentDTO
                {
                    AssessmentId = assessment.AssessmentId,
                    Title = assessment.Title,
                    Description = assessment.Description,
                    CourseId = assessment.CourseId
                }).ToList();

                // Serialize and store the data in cache
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache expiration time
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(assessmentsDtos), cacheOptions);

                _logger.LogInformation("Returning assessments from database and storing in cache");
                return assessmentsDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all assessments from repository");
                throw;
            }
        }

        /// <summary>
        /// Retrieves an assessment by its ID.
        /// </summary>
        /// <param name="assessmentId">The ID of the assessment to retrieve.</param>
        /// <returns>An AssessmentDTO object.</returns>
        public async Task<AssessmentDTO> GetAssessmentByIdAsync(int assessmentId)
        {
            try
            {
                _logger.LogInformation("Fetching assessment with ID {AssessmentId} from repository", assessmentId);

                // Redis cache key for specific assessment
                var cacheKey = $"{CacheKeyPrefix}{assessmentId}";

                // Try to get the cached value
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Returning cached assessment data for ID {AssessmentId}", assessmentId);
                    return JsonSerializer.Deserialize<AssessmentDTO>(cachedData);
                }

                _logger.LogInformation("Assessment not found in cache, fetching from database");

                // Fetch assessment from repository
                var assessment = await _assessmentRepository.GetByIdAsync(assessmentId);
                if (assessment == null)
                {
                    _logger.LogWarning("Assessment with ID {AssessmentId} not found", assessmentId);
                    return null;
                }

                // Convert to DTO
                var assessmentDto = new AssessmentDTO
                {
                    AssessmentId = assessment.AssessmentId,
                    Title = assessment.Title,
                    Description = assessment.Description,
                    CourseId = assessment.CourseId
                };

                // Serialize and store the data in cache
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache expiration time
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(assessmentDto), cacheOptions);

                _logger.LogInformation("Returning assessment with ID {AssessmentId} from database and storing in cache", assessmentId);
                return assessmentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching assessment with ID {AssessmentId} from repository", assessmentId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new assessment.
        /// </summary>
        /// <param name="createAssessmentDto">The data for the new assessment.</param>
        /// <returns>The ID of the newly created assessment.</returns>
        public async Task<int> CreateAssessmentAsync(CreateAssessmentDTO createAssessmentDto)
        {
            try
            {
                _logger.LogInformation("Creating a new assessment in repository");

                // Map DTO to entity
                var assessment = new Assessment
                {
                    Title = createAssessmentDto.Title,
                    Description = createAssessmentDto.Description,
                    CourseId = createAssessmentDto.CourseId
                };

                await _assessmentRepository.AddAsync(assessment);
                _logger.LogInformation("Successfully created assessment with ID {AssessmentId} in repository", assessment.AssessmentId);

                // Invalidate cache for all assessments
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");
                _logger.LogInformation("Cache cleared after creating new assessment");

                return assessment.AssessmentId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new assessment in repository");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing assessment.
        /// </summary>
        /// <param name="assessmentId">The ID of the assessment to update.</param>
        /// <param name="updateAssessmentDto">The updated data for the assessment.</param>
        /// <returns>A boolean indicating whether the update was successful.</returns>
        public async Task<bool> UpdateAssessmentAsync(int assessmentId, UpdateAssessmentDTO updateAssessmentDto)
        {
            try
            {
                _logger.LogInformation("Updating assessment with ID {AssessmentId} in repository", assessmentId);

                // Fetch the existing assessment
                var assessment = await _assessmentRepository.GetByIdAsync(assessmentId);
                if (assessment == null)
                {
                    _logger.LogWarning("Assessment with ID {AssessmentId} not found in repository", assessmentId);
                    return false;
                }

                // Update the assessment entity
                assessment.Title = updateAssessmentDto.Title;
                assessment.Description = updateAssessmentDto.Description;
                assessment.CourseId = updateAssessmentDto.CourseId;
                assessment.UpdatedAt = DateTime.UtcNow;

                await _assessmentRepository.UpdateAsync(assessment);
                _logger.LogInformation("Successfully updated assessment with ID {AssessmentId} in repository", assessmentId);

                // Invalidate cache for this assessment and all assessments
                await _cache.RemoveAsync($"{CacheKeyPrefix}{assessmentId}");
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");
                _logger.LogInformation("Cache cleared after updating assessment with ID {AssessmentId}", assessmentId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating assessment with ID {AssessmentId} in repository", assessmentId);
                throw;
            }
        }

        /// <summary>
        /// Deletes an assessment by its ID.
        /// </summary>
        /// <param name="assessmentId">The ID of the assessment to delete.</param>
        /// <returns>A boolean indicating whether the deletion was successful.</returns>
        public async Task<bool> DeleteAssessmentAsync(int assessmentId)
        {
            try
            {
                _logger.LogInformation("Deleting assessment with ID {AssessmentId} from repository", assessmentId);

                // Fetch the existing assessment
                var assessment = await _assessmentRepository.GetByIdAsync(assessmentId);
                if (assessment == null)
                {
                    _logger.LogWarning("Assessment with ID {AssessmentId} not found in repository", assessmentId);
                    return false;
                }

                await _assessmentRepository.DeleteAsync(assessmentId);
                _logger.LogInformation("Successfully deleted assessment with ID {AssessmentId} from repository", assessmentId);

                // Invalidate cache for this assessment and all assessments
                await _cache.RemoveAsync($"{CacheKeyPrefix}{assessmentId}");
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");
                _logger.LogInformation("Cache cleared after deleting assessment with ID {AssessmentId}", assessmentId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting assessment with ID {AssessmentId} from repository", assessmentId);
                throw;
            }
        }
    }
}
