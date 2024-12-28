using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMS.Application.Services
{
    public class DiscussionForumService : IDiscussionForumService
    {
        private readonly IDiscussionForumRepository _discussionForumRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<DiscussionForumService> _logger;
        private const string CacheKeyPrefix = "DiscussionForum_";

        public DiscussionForumService(
            IDiscussionForumRepository discussionForumRepository,
            IDistributedCache cache,
            ILogger<DiscussionForumService> logger)
        {
            _discussionForumRepository = discussionForumRepository ?? throw new ArgumentNullException(nameof(discussionForumRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all discussion forums.
        /// </summary>
        /// <returns>A collection of DiscussionForumDTO objects.</returns>
        public async Task<IEnumerable<DiscussionForumDTO>> GetAllDiscussionForumsAsync()
        {
            _logger.LogInformation("Initiating request to fetch all discussion forums.");

            try
            {
                // Check if data is available in the cache
                var cacheKey = $"{CacheKeyPrefix}All";
                var cachedData = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Returning cached discussion forums data.");
                    return JsonSerializer.Deserialize<IEnumerable<DiscussionForumDTO>>(cachedData);
                }

                _logger.LogInformation("Discussion forums not found in cache, fetching from database.");

                // Fetch the data from the repository
                var forums = await _discussionForumRepository.GetAllAsync();
                if (forums == null || !forums.Any())
                {
                    _logger.LogInformation("No discussion forums found in the database.");
                    return Enumerable.Empty<DiscussionForumDTO>();
                }

                _logger.LogInformation("Successfully fetched {DiscussionForumCount} discussion forums from the database.", forums.Count());

                // Map entities to DTOs
                var forumDTOs = MapToDTOs(forums);

                // Store the data in cache
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(forumDTOs), cacheOptions);

                _logger.LogInformation("Returning discussion forums from database and storing in cache.");
                return forumDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all discussion forums.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific discussion forum by its ID.
        /// </summary>
        /// <param name="discussionForumId">The ID of the discussion forum to retrieve.</param>
        /// <returns>A DiscussionForumDTO object representing the discussion forum.</returns>
        public async Task<DiscussionForumDTO> GetDiscussionForumByIdAsync(int discussionForumId)
        {
            _logger.LogInformation("Initiating request to fetch discussion forum with ID {DiscussionForumId}.", discussionForumId);

            try
            {
                // Check if data is available in the cache
                var cacheKey = $"{CacheKeyPrefix}{discussionForumId}";
                var cachedData = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Returning cached data for discussion forum ID {DiscussionForumId}.", discussionForumId);
                    return JsonSerializer.Deserialize<DiscussionForumDTO>(cachedData);
                }

                _logger.LogInformation("Discussion forum not found in cache, fetching from database.");

                // Fetch the data from the repository
                var forum = await _discussionForumRepository.GetByIdAsync(discussionForumId);
                if (forum == null)
                {
                    _logger.LogWarning("Discussion forum with ID {DiscussionForumId} not found in repository.", discussionForumId);
                    return null;
                }

                // Map entity to DTO
                var forumDTO = MapToDTO(forum);

                // Store the data in cache
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(forumDTO), cacheOptions);

                _logger.LogInformation("Returning discussion forum with ID {DiscussionForumId} from database and storing in cache.", discussionForumId);
                return forumDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching discussion forum with ID {DiscussionForumId}.", discussionForumId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new discussion forum.
        /// </summary>
        /// <param name="createForumDTO">The details of the discussion forum to create.</param>
        /// <returns>The ID of the newly created discussion forum.</returns>
        public async Task<int> CreateDiscussionForumAsync(CreateDiscussionForumDTO createForumDTO)
        {
            _logger.LogInformation("Initiating request to create a new discussion forum.");

            try
            {
                // Map DTO to entity
                var forum = new DiscussionForum
                {
                    Title = createForumDTO.Title,
                    Description = createForumDTO.Description,
                    CreatedBy = createForumDTO.CreatedBy,
                    CourseId = createForumDTO.CourseId
                };

                // Add the new forum to the repository
                await _discussionForumRepository.AddAsync(forum);
                _logger.LogInformation("Successfully created discussion forum with ID {DiscussionForumId}.", forum.ForumId);

                // Clear cache for all discussion forums
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");
                _logger.LogInformation("Cache cleared after creating discussion forum with ID {DiscussionForumId}.", forum.ForumId);

                return forum.ForumId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new discussion forum.");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing discussion forum.
        /// </summary>
        /// <param name="updateForumDTO">The updated details of the discussion forum.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateDiscussionForumAsync(UpdateDiscussionForumDTO updateForumDTO)
        {
            _logger.LogInformation("Initiating request to update discussion forum with ID {DiscussionForumId}.", updateForumDTO.ForumId);

            try
            {
                // Fetch the existing forum
                var existingForum = await _discussionForumRepository.GetByIdAsync(updateForumDTO.ForumId);
                if (existingForum == null)
                {
                    _logger.LogWarning("Discussion forum with ID {DiscussionForumId} not found in repository.", updateForumDTO.ForumId);
                    return false;
                }

                // Update the forum properties
                existingForum.Title = updateForumDTO.Title;
                existingForum.Description = updateForumDTO.Description;
                existingForum.CourseId = updateForumDTO.CourseId;
                existingForum.UpdatedAt = DateTime.UtcNow;

                // Save the updated forum
                await _discussionForumRepository.UpdateAsync(existingForum);
                _logger.LogInformation("Successfully updated discussion forum with ID {DiscussionForumId}.", updateForumDTO.ForumId);

                // Clear cache for this discussion forum and all discussion forums list
                await _cache.RemoveAsync($"{CacheKeyPrefix}{updateForumDTO.ForumId}");
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");
                _logger.LogInformation("Cache cleared after updating discussion forum with ID {DiscussionForumId}.", updateForumDTO.ForumId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating discussion forum with ID {DiscussionForumId}.", updateForumDTO.ForumId);
                throw;
            }
        }

        /// <summary>
        /// Deletes a discussion forum by its ID.
        /// </summary>
        /// <param name="discussionForumId">The ID of the discussion forum to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteDiscussionForumAsync(int discussionForumId)
        {
            _logger.LogInformation("Initiating request to delete discussion forum with ID {DiscussionForumId}.", discussionForumId);

            try
            {
                // Fetch the existing forum
                var existingForum = await _discussionForumRepository.GetByIdAsync(discussionForumId);
                if (existingForum == null)
                {
                    _logger.LogWarning("Discussion forum with ID {DiscussionForumId} not found in repository.", discussionForumId);
                    return false;
                }

                // Delete the forum
                await _discussionForumRepository.DeleteAsync(discussionForumId);
                _logger.LogInformation("Successfully deleted discussion forum with ID {DiscussionForumId}.", discussionForumId);

                // Clear cache for this discussion forum and all discussion forums list
                await _cache.RemoveAsync($"{CacheKeyPrefix}{discussionForumId}");
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");
                _logger.LogInformation("Cache cleared after deleting discussion forum with ID {DiscussionForumId}.", discussionForumId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting discussion forum with ID {DiscussionForumId}.", discussionForumId);
                throw;
            }
        }

        /// <summary>
        /// Maps a DiscussionForum entity to a DiscussionForumDTO.
        /// </summary>
        /// <param name="forum">The DiscussionForum entity to map.</param>
        /// <returns>The corresponding DiscussionForumDTO.</returns>
        private DiscussionForumDTO MapToDTO(DiscussionForum forum)
        {
            return new DiscussionForumDTO
            {
                ForumId = forum.ForumId,
                Title = forum.Title,
                Description = forum.Description,
                CreatedAt = forum.CreatedAt,
                UpdatedAt = forum.UpdatedAt,
                CreatedBy = forum.CreatedBy,
                CourseId = forum.CourseId
            };
        }

        /// <summary>
        /// Maps a collection of DiscussionForum entities to a collection of DiscussionForumDTOs.
        /// </summary>
        /// <param name="forums">The collection of DiscussionForum entities to map.</param>
        /// <returns>A collection of DiscussionForumDTOs.</returns>
        private IEnumerable<DiscussionForumDTO> MapToDTOs(IEnumerable<DiscussionForum> forums)
        {
            var forumDTOs = new List<DiscussionForumDTO>();
            foreach (var forum in forums)
            {
                forumDTOs.Add(MapToDTO(forum));
            }
            return forumDTOs;
        }
    }
}
