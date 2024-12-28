using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMS.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CourseService> _logger;
        private const string CacheKey = "courses";

        public CourseService(ICourseRepository courseRepository, ILogger<CourseService> logger, IDistributedCache cache)
        {
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Retrieves all courses, either from the cache or the database.
        /// </summary>
        /// <returns>A collection of CourseDTO objects.</returns>
        public async Task<IEnumerable<CourseDTO>> GetAllCoursesAsync()
        {
            _logger.LogInformation("Initiating request to fetch all courses");

            try
            {
                // Attempt to retrieve courses from the cache
                string cachedCourses = await _cache.GetStringAsync(CacheKey);

                // If cached data is found, deserialize and return it
                if (!string.IsNullOrEmpty(cachedCourses))
                {
                    _logger.LogInformation("Courses retrieved from cache");
                    return JsonSerializer.Deserialize<IEnumerable<CourseDTO>>(cachedCourses);
                }

                _logger.LogInformation("Courses not found in cache, fetching from database");

                // Fetch data from the database if not found in the cache
                var courses = await _courseRepository.GetAllAsync();

                // Check if any courses were retrieved
                if (courses == null || !courses.Any())
                {
                    _logger.LogInformation("No courses found in the database");
                    return Enumerable.Empty<CourseDTO>();
                }

                _logger.LogInformation("Fetched {CourseCount} courses from database", courses.Count());

                // Convert entity list to DTO list
                var coursesDtos = courses.Select(course => new CourseDTO
                {
                    CourseId = course.CourseId,
                    CourseName = course.Title,
                    Description = course.Description,
                    Modules = course.Modules.Select(m => new ModuleDTO
                    {
                        ModuleId = m.ModuleId,
                        ModuleName = m.Title
                    }).ToList()
                }).ToList();

                // Serialize the DTO list and cache it
                string serializedCourses = JsonSerializer.Serialize(coursesDtos);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10), // Set cache expiration
                    SlidingExpiration = TimeSpan.FromMinutes(2) // Set sliding expiration
                };
                await _cache.SetStringAsync(CacheKey, serializedCourses, cacheOptions);

                _logger.LogInformation("Courses data cached successfully");
                return coursesDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all courses");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific course by its ID, either from the cache or the database.
        /// </summary>
        /// <param name="courseId">The ID of the course to retrieve.</param>
        /// <returns>The CourseDTO object for the specified ID.</returns>
        public async Task<CourseDTO> GetCourseByIdAsync(int courseId)
        {
            _logger.LogInformation("Initiating request to fetch course with ID {CourseId}", courseId);

            try
            {
                // Create a cache key for the specific course
                string cacheKey = $"course_{courseId}";
                // Attempt to retrieve course data from the cache
                string cachedCourse = await _cache.GetStringAsync(cacheKey);

                // If cached data is found, deserialize and return it
                if (!string.IsNullOrEmpty(cachedCourse))
                {
                    _logger.LogInformation("Returning cached data for course ID {CourseId}", courseId);
                    return JsonSerializer.Deserialize<CourseDTO>(cachedCourse);
                }

                _logger.LogInformation("Course not found in cache, fetching from database");

                // Fetch course from the database if not found in the cache
                var course = await _courseRepository.GetByIdAsync(courseId);

                // Check if the course exists
                if (course == null)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found", courseId);
                    return null;
                }

                // Convert the course entity to a DTO
                var courseDto = new CourseDTO
                {
                    CourseId = course.CourseId,
                    CourseName = course.Title,
                    Description = course.Description,
                    Modules = course.Modules.Select(m => new ModuleDTO
                    {
                        ModuleId = m.ModuleId,
                        ModuleName = m.Title
                    }).ToList()
                };

                // Serialize the course DTO and cache it
                string serializedCourse = JsonSerializer.Serialize(courseDto);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10), // Set cache expiration
                    SlidingExpiration = TimeSpan.FromMinutes(2) // Set sliding expiration
                };
                await _cache.SetStringAsync(cacheKey, serializedCourse, cacheOptions);

                _logger.LogInformation("Course ID {CourseId} fetched and cached successfully", courseId);
                return courseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching course with ID {CourseId}", courseId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new course in the repository and clears the cache.
        /// </summary>
        /// <param name="createCourseDto">The details of the course to create.</param>
        /// <returns>The ID of the newly created course.</returns>
        public async Task<int> CreateCourseAsync(CreateCourseDTO createCourseDto)
        {
            _logger.LogInformation("Initiating creation of a new course");

            try
            {
                // Create a new Course entity from DTO
                var course = new Course
                {
                    Title = createCourseDto.CourseName,
                    Description = createCourseDto.Description
                };

                // Add the new course to the repository
                await _courseRepository.AddAsync(course);

                _logger.LogInformation("Successfully created course with ID {CourseId}", course.CourseId);

                // Clear the entire courses cache as a new course has been added
                await _cache.RemoveAsync(CacheKey);
                _logger.LogInformation("Cache cleared after creating course");

                return course.CourseId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new course");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing course in the repository and clears the cache.
        /// </summary>
        /// <param name="courseId">The ID of the course to update.</param>
        /// <param name="updateCourseDto">The updated details of the course.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        public async Task<bool> UpdateCourseAsync(int courseId, UpdateCourseDTO updateCourseDto)
        {
            _logger.LogInformation("Initiating update for course with ID {CourseId}", courseId);

            try
            {
                // Fetch the existing course from the repository
                var course = await _courseRepository.GetByIdAsync(courseId);

                // Check if the course exists
                if (course == null)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found", courseId);
                    return false;
                }

                // Update course details
                course.Title = updateCourseDto.CourseName;
                course.Description = updateCourseDto.Description;
                course.UpdatedAt = DateTime.UtcNow;

                // Save the changes to the repository
                await _courseRepository.UpdateAsync(course);
                _logger.LogInformation("Successfully updated course with ID {CourseId}", courseId);

                // Clear the cache for the entire list and specific course
                await _cache.RemoveAsync(CacheKey);
                var cacheKey = $"course_{courseId}";
                await _cache.RemoveAsync(cacheKey);
                _logger.LogInformation("Cache cleared after updating course with ID {CourseId}", courseId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating course with ID {CourseId}", courseId);
                throw;
            }
        }

        /// <summary>
        /// Deletes a course by ID from the repository and clears the cache.
        /// </summary>
        /// <param name="courseId">The ID of the course to delete.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            _logger.LogInformation("Initiating deletion of course with ID {CourseId}", courseId);

            try
            {
                // Fetch the course to be deleted
                var course = await _courseRepository.GetByIdAsync(courseId);

                // Check if the course exists
                if (course == null)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found", courseId);
                    return false;
                }

                // Delete the course from the repository
                await _courseRepository.DeleteAsync(courseId);
                _logger.LogInformation("Successfully deleted course with ID {CourseId}", courseId);

                // Clear the cache for the entire list and specific course
                await _cache.RemoveAsync(CacheKey);
                var cacheKey = $"course_{courseId}";
                await _cache.RemoveAsync(cacheKey);
                _logger.LogInformation("Cache cleared after deleting course with ID {CourseId}", courseId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting course with ID {CourseId}", courseId);
                throw;
            }
        }
    }
}
