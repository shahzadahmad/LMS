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
    /// Service class for managing announcements in the LMS application.
    /// </summary>
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<AnnouncementService> _logger;
        private const string CacheKeyPrefix = "Announcement_";

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnouncementService"/> class.
        /// </summary>
        /// <param name="announcementRepository">The repository for accessing announcement data.</param>
        /// <param name="cache">The distributed cache for caching announcement data.</param>
        /// <param name="logger">The logger for logging announcement-related operations.</param>
        public AnnouncementService(
            IAnnouncementRepository announcementRepository,
            IDistributedCache cache,
            ILogger<AnnouncementService> logger)
        {
            _announcementRepository = announcementRepository;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all announcements from the system, with caching to improve performance.
        /// </summary>
        /// <returns>A collection of <see cref="AnnouncementDTO"/> objects representing all announcements.</returns>
        public async Task<IEnumerable<AnnouncementDTO>> GetAllAnnouncementsAsync()
        {
            try
            {
                _logger.LogInformation("Attempting to fetch all announcements.");

                var cacheKey = $"{CacheKeyPrefix}All";
                var cachedData = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Announcements retrieved from cache.");
                    return JsonSerializer.Deserialize<IEnumerable<AnnouncementDTO>>(cachedData);
                }

                _logger.LogInformation("Cache miss. Fetching announcements from the database.");

                var announcements = await _announcementRepository.GetAllAsync();
                if (announcements == null || !announcements.Any())
                {
                    _logger.LogInformation("No announcements found in the database.");
                    return Enumerable.Empty<AnnouncementDTO>();
                }

                _logger.LogInformation("Fetched {AnnouncementCount} announcements from the database.", announcements.Count());

                var announcementDTOs = MapToDTOs(announcements);
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)); // Cache expiration    

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(announcementDTOs), cacheOptions);

                _logger.LogInformation("Announcements stored in cache.");
                return announcementDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all announcements.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific announcement by its ID, with caching for faster access.
        /// </summary>
        /// <param name="announcementId">The ID of the announcement to retrieve.</param>
        /// <returns>A <see cref="AnnouncementDTO"/> representing the announcement, or null if not found.</returns>
        public async Task<AnnouncementDTO> GetAnnouncementByIdAsync(int announcementId)
        {
            try
            {
                _logger.LogInformation("Attempting to fetch announcement with ID {AnnouncementId}.", announcementId);

                var cacheKey = $"{CacheKeyPrefix}{announcementId}";
                var cachedData = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Announcement with ID {AnnouncementId} retrieved from cache.", announcementId);
                    return JsonSerializer.Deserialize<AnnouncementDTO>(cachedData);
                }

                _logger.LogInformation("Cache miss. Fetching announcement with ID {AnnouncementId} from the database.", announcementId);

                var announcement = await _announcementRepository.GetByIdAsync(announcementId);
                if (announcement == null)
                {
                    _logger.LogWarning("Announcement with ID {AnnouncementId} not found.", announcementId);
                    return null;
                }

                var announcementDTO = MapToDTO(announcement);

                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(announcementDTO), cacheOptions);

                _logger.LogInformation("Announcement with ID {AnnouncementId} stored in cache.", announcementId);
                return announcementDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching announcement with ID {AnnouncementId}.", announcementId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new announcement in the system and invalidates the related cache entries.
        /// </summary>
        /// <param name="createAnnouncementDTO">The data transfer object containing the details of the announcement to create.</param>
        /// <returns>The ID of the newly created announcement.</returns>
        public async Task<int> CreateAnnouncementAsync(CreateAnnouncementDTO createAnnouncementDTO)
        {
            try
            {
                _logger.LogInformation("Attempting to create a new announcement.");

                var announcement = new Announcement
                {
                    Title = createAnnouncementDTO.Title,
                    Content = createAnnouncementDTO.Content,
                    CreatedBy = createAnnouncementDTO.CreatedBy,
                    CourseId = createAnnouncementDTO.CourseId
                };

                await _announcementRepository.AddAsync(announcement);
                _logger.LogInformation("Announcement with ID {AnnouncementId} created successfully.", announcement.AnnouncementId);

                // Clear cache for all announcements
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");
                _logger.LogInformation("Cache cleared after creating announcement.");

                return announcement.AnnouncementId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new announcement.");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing announcement in the system and invalidates the related cache entries.
        /// </summary>
        /// <param name="updateAnnouncementDTO">The data transfer object containing the updated announcement details.</param>
        /// <returns>A boolean indicating whether the update was successful.</returns>
        public async Task<bool> UpdateAnnouncementAsync(UpdateAnnouncementDTO updateAnnouncementDTO)
        {
            try
            {
                _logger.LogInformation("Attempting to update announcement with ID {AnnouncementId}.", updateAnnouncementDTO.AnnouncementId);

                var existingAnnouncement = await _announcementRepository.GetByIdAsync(updateAnnouncementDTO.AnnouncementId);
                if (existingAnnouncement == null)
                {
                    _logger.LogWarning("Announcement with ID {AnnouncementId} not found.", updateAnnouncementDTO.AnnouncementId);
                    return false;
                }

                existingAnnouncement.Title = updateAnnouncementDTO.Title;
                existingAnnouncement.Content = updateAnnouncementDTO.Content;
                existingAnnouncement.CourseId = updateAnnouncementDTO.CourseId;
                existingAnnouncement.UpdatedAt = DateTime.UtcNow;

                await _announcementRepository.UpdateAsync(existingAnnouncement);
                _logger.LogInformation("Announcement with ID {AnnouncementId} updated successfully.", updateAnnouncementDTO.AnnouncementId);

                // Clear cache for this announcement and all announcements
                await _cache.RemoveAsync($"{CacheKeyPrefix}{updateAnnouncementDTO.AnnouncementId}");
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");
                _logger.LogInformation("Cache cleared after updating announcement with ID {AnnouncementId}.", updateAnnouncementDTO.AnnouncementId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating announcement with ID {AnnouncementId}.", updateAnnouncementDTO.AnnouncementId);
                throw;
            }
        }

        /// <summary>
        /// Deletes an announcement from the system and invalidates the related cache entries.
        /// </summary>
        /// <param name="announcementId">The ID of the announcement to delete.</param>
        /// <returns>A boolean indicating whether the deletion was successful.</returns>
        public async Task<bool> DeleteAnnouncementAsync(int announcementId)
        {
            try
            {
                _logger.LogInformation("Attempting to delete announcement with ID {AnnouncementId}.", announcementId);

                var existingAnnouncement = await _announcementRepository.GetByIdAsync(announcementId);
                if (existingAnnouncement == null)
                {
                    _logger.LogWarning("Announcement with ID {AnnouncementId} not found.", announcementId);
                    return false;
                }

                await _announcementRepository.DeleteAsync(announcementId);
                _logger.LogInformation("Announcement with ID {AnnouncementId} deleted successfully.", announcementId);

                // Clear cache for this announcement and all announcements
                await _cache.RemoveAsync($"{CacheKeyPrefix}{announcementId}");
                await _cache.RemoveAsync($"{CacheKeyPrefix}All");
                _logger.LogInformation("Cache cleared after deleting announcement with ID {AnnouncementId}.", announcementId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting announcement with ID {AnnouncementId}.", announcementId);
                throw;
            }
        }

        /// <summary>
        /// Maps an <see cref="Announcement"/> entity to an <see cref="AnnouncementDTO"/>.
        /// </summary>
        /// <param name="announcement">The announcement entity to map.</param>
        /// <returns>An <see cref="AnnouncementDTO"/> representing the mapped announcement.</returns>
        private AnnouncementDTO MapToDTO(Announcement announcement)
        {
            return new AnnouncementDTO
            {
                AnnouncementId = announcement.AnnouncementId,
                Title = announcement.Title,
                Content = announcement.Content,
                CreatedAt = announcement.CreatedAt,
                CreatedBy = announcement.CreatedBy,
                CourseId = announcement.CourseId
            };
        }

        /// <summary>
        /// Maps a collection of <see cref="Announcement"/> entities to a collection of <see cref="AnnouncementDTO"/> objects.
        /// </summary>
        /// <param name="announcements">The collection of announcement entities to map.</param>
        /// <returns>A collection of <see cref="AnnouncementDTO"/> objects representing the mapped announcements.</returns>
        private IEnumerable<AnnouncementDTO> MapToDTOs(IEnumerable<Announcement> announcements)
        {
            var announcementDTOs = new List<AnnouncementDTO>();
            foreach (var announcement in announcements)
            {
                announcementDTOs.Add(MapToDTO(announcement));
            }
            return announcementDTOs;
        }
    }
}
