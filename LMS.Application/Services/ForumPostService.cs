using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMS.Application.Services
{

    public class ForumPostService : IForumPostService
    {
        private readonly IForumPostRepository _forumPostRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ForumPostService> _logger;

        public ForumPostService(
            IForumPostRepository forumPostRepository,
            IDistributedCache cache,
            ILogger<ForumPostService> logger)
        {
            _forumPostRepository = forumPostRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ForumPostDTO> GetForumPostByIdAsync(int postId)
        {
            try
            {
                _logger.LogInformation($"Fetching forum post with ID {postId} from repository");

                var cacheKey = $"ForumPost_{postId}";
                var cachedPost = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedPost))
                {
                    _logger.LogInformation($"Forum post with ID {postId} retrieved from cache.");
                    return JsonSerializer.Deserialize<ForumPostDTO>(cachedPost);
                }

                _logger.LogInformation($"Retrieving forum post with ID {postId} from the database.");
                var forumPost = await _forumPostRepository.GetByIdAsync(postId);
                if (forumPost == null)
                {
                    _logger.LogWarning($"Forum post with ID {postId} not found in repository");
                    return null;
                }


                var forumPostDto = MapToDTO(forumPost);

                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(forumPostDto), cacheOptions);

                _logger.LogInformation("Returning forum post {ForumPostId} from database and storing in cache.", postId);
                return forumPostDto;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving forum post with ID {postId} from repository");
                throw;
            }
        }

        public async Task<IEnumerable<ForumPostDTO>> GetAllForumPostsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all forum posts from repository");

                var cacheKey = "ForumPosts";
                var cachedPosts = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedPosts))
                {
                    _logger.LogInformation("Forum posts retrieved from cache.");
                    return JsonSerializer.Deserialize<IEnumerable<ForumPostDTO>>(cachedPosts);
                }

                _logger.LogInformation("Retrieving all forum posts from the database.");
                var forumPosts = await _forumPostRepository.GetAllAsync();
                if (forumPosts == null || !forumPosts.Any())
                {
                    // Handle case where no forum posts are found
                    // Optionally log this situation
                    _logger.LogInformation("No forum posts found in the database.");
                    return Enumerable.Empty<ForumPostDTO>();
                }

                _logger.LogInformation("Successfully fetched {ForumPostCount} forum posts from database", forumPosts.Count());

                var forumPostsDto = MapToDTO(forumPosts);
                var cacheOptions = new DistributedCacheEntryOptions()
                                   .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(forumPostsDto), cacheOptions);

                _logger.LogInformation("Returning forum posts from database and storing in cache");
                return forumPostsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all forum posts.");
                throw;
            }
        }

        public async Task<int> CreateForumPostAsync(CreateForumPostDTO createForumPostDto)
        {
            try
            {
                _logger.LogInformation("Creating a new forum post in repository");

                var forumPost = new ForumPost
                {
                    ForumId = createForumPostDto.ForumId,
                    Content = createForumPostDto.Content,
                    CreatedBy = createForumPostDto.CreatedBy,
                    ParentPostId = createForumPostDto.ParentPostId
                };

                await _forumPostRepository.AddAsync(forumPost);
                _logger.LogInformation("Successfully created forum post with ID {ForumPostId} in repository", forumPost.ForumId);

                // Clear cache for all forum posts
                await _cache.RemoveAsync("ForumPosts");
                _logger.LogInformation("Cache cleared after creating forum post in repository");

                return forumPost.PostId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a forum post in repository");
                throw;
            }
        }

        public async Task<bool> UpdateForumPostAsync(int postId, UpdateForumPostDTO updateForumPostDto)
        {
            try
            {
                _logger.LogInformation($"Updating forum post with ID {postId} in repository");

                var forumPost = await _forumPostRepository.GetByIdAsync(postId);
                if (forumPost == null)
                {
                    _logger.LogWarning($"Forum post with ID {postId} not found in repository");
                    return false;
                }

                forumPost.Content = updateForumPostDto.Content;
                forumPost.UpdatedAt = DateTime.UtcNow;

                await _forumPostRepository.UpdateAsync(forumPost);
                _logger.LogInformation("Successfully updated forum post with id {ForumPostId} in repository", postId);

                // Clear cache for this forum post and all forum posts list
                await _cache.RemoveAsync($"ForumPost_{postId}");
                await _cache.RemoveAsync("ForumPosts");
                _logger.LogInformation("Cache cleared after updating forum post with id {ForumPostId} in repository", postId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating forum post with ID {postId} in repository");
                throw;
            }
        }

        public async Task<bool> DeleteForumPostAsync(int postId)
        {
            try
            {
                _logger.LogInformation($"Deleting forum post with ID {postId} from repository");

                var forumPost = await _forumPostRepository.GetByIdAsync(postId);
                if (forumPost == null)
                {
                    _logger.LogWarning($"Forum post with ID {postId} not found in repository");
                    return false;
                }

                await _forumPostRepository.DeleteAsync(postId);
                _logger.LogInformation("Successfully deleted forum post with id {ForumPostId} from repository", postId);

                // Clear cache for this forum post and all forum posts list
                await _cache.RemoveAsync($"ForumPost_{postId}");
                await _cache.RemoveAsync("ForumPosts");
                _logger.LogInformation("Cache cleared after updating forum post with id {ForumPostId} in repository", postId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting forum post with ID {postId} from repository");
                throw;
            }
        }

        // Mapping helper methods
        private ForumPostDTO MapToDTO(ForumPost forumPost)
        {
            return new ForumPostDTO
            {
                PostId = forumPost.PostId,
                ForumId = forumPost.ForumId,
                Content = forumPost.Content,
                CreatedAt = forumPost.CreatedAt,
                UpdatedAt = forumPost.UpdatedAt,
                CreatedBy = forumPost.CreatedBy,
                ParentPostId = forumPost.ParentPostId
            };
        }

        private IEnumerable<ForumPostDTO> MapToDTO(IEnumerable<ForumPost> forumPosts)
        {
            var forumPostsDto = new List<ForumPostDTO>();
            foreach (var post in forumPosts)
            {
                forumPostsDto.Add(MapToDTO(post));
            }
            return forumPostsDto;
        }
    }

}