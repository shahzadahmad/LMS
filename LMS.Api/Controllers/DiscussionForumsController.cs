using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Api.Controllers
{
    [Authorize(Roles = "Admin, Instructor")] // Ensure only Admin or Instructor can access this controller
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("GlobalRateLimitPolicy")] // Apply rate limiting policy to the entire controller
    public class DiscussionForumsController : ControllerBase
    {
        private readonly IDiscussionForumService _discussionForumService;
        private readonly ILogger<DiscussionForumsController> _logger;

        public DiscussionForumsController(IDiscussionForumService discussionForumService, ILogger<DiscussionForumsController> logger)
        {
            _discussionForumService = discussionForumService ?? throw new ArgumentNullException(nameof(discussionForumService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all discussion forums.
        /// </summary>
        /// <returns>A list of DiscussionForumDTO objects.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllDiscussionForums()
        {
            _logger.LogInformation("Initiating request to fetch all discussion forums.");

            try
            {
                var discussionForums = await _discussionForumService.GetAllDiscussionForumsAsync();

                if (discussionForums == null || !discussionForums.Any())
                {
                    _logger.LogInformation("No discussion forums available.");
                    return NotFound(new { Message = "No discussion forums available." });
                }

                _logger.LogInformation("Successfully fetched {DiscussionForumCount} discussion forums.", discussionForums.Count());
                return Ok(discussionForums);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all discussion forums.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves a discussion forum by its ID.
        /// </summary>
        /// <param name="id">The ID of the discussion forum to retrieve.</param>
        /// <returns>A DiscussionForumDTO object.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDiscussionForumById(int id)
        {
            _logger.LogInformation("Initiating request to fetch discussion forum with ID {DiscussionForumId}.", id);

            try
            {
                var discussionForum = await _discussionForumService.GetDiscussionForumByIdAsync(id);

                if (discussionForum == null)
                {
                    _logger.LogWarning("Discussion forum with ID {DiscussionForumId} not found.", id);
                    return NotFound(new { Message = $"Discussion forum with ID {id} not found." });
                }

                _logger.LogInformation("Successfully fetched discussion forum with ID {DiscussionForumId}.", id);
                return Ok(discussionForum);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching discussion forum with ID {DiscussionForumId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new discussion forum.
        /// </summary>
        /// <param name="createDiscussionForumDTO">The details of the discussion forum to create.</param>
        /// <returns>The ID of the newly created discussion forum.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateDiscussionForum([FromBody] CreateDiscussionForumDTO createDiscussionForumDTO)
        {
            _logger.LogInformation("Initiating request to create a new discussion forum.");

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for the CreateDiscussionForumDTO object.");
                    return BadRequest(ModelState);
                }

                var discussionForumId = await _discussionForumService.CreateDiscussionForumAsync(createDiscussionForumDTO);
                _logger.LogInformation("Successfully created discussion forum with ID {DiscussionForumId}.", discussionForumId);

                return CreatedAtAction(nameof(GetDiscussionForumById), new { id = discussionForumId }, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new discussion forum.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing discussion forum.
        /// </summary>
        /// <param name="id">The ID of the discussion forum to update.</param>
        /// <param name="updateDiscussionForumDto">The updated details of the discussion forum.</param>
        /// <returns>No content if successful; otherwise, an appropriate error response.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiscussionForum(int id, [FromBody] UpdateDiscussionForumDTO updateDiscussionForumDto)
        {
            _logger.LogInformation("Initiating request to update discussion forum with ID {DiscussionForumId}.", id);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for the UpdateDiscussionForumDTO object.");
                    return BadRequest(ModelState);
                }

                var existingForum = await _discussionForumService.GetDiscussionForumByIdAsync(id);
                if (existingForum == null)
                {
                    _logger.LogWarning("Discussion forum with ID {DiscussionForumId} not found.", id);
                    return NotFound(new { Message = $"Discussion forum with ID {id} not found." });
                }

                var result = await _discussionForumService.UpdateDiscussionForumAsync(updateDiscussionForumDto);
                if (!result)
                {
                    _logger.LogWarning("Failed to update discussion forum with ID {DiscussionForumId}.", id);
                    return NotFound(new { Message = $"Failed to update discussion forum with ID {id}." });
                }

                _logger.LogInformation("Successfully updated discussion forum with ID {DiscussionForumId}.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating discussion forum with ID {DiscussionForumId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a discussion forum by its ID.
        /// </summary>
        /// <param name="id">The ID of the discussion forum to delete.</param>
        /// <returns>No content if successful; otherwise, an appropriate error response.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscussionForum(int id)
        {
            _logger.LogInformation("Initiating request to delete discussion forum with ID {DiscussionForumId}.", id);

            try
            {
                var existingForum = await _discussionForumService.GetDiscussionForumByIdAsync(id);
                if (existingForum == null)
                {
                    _logger.LogWarning("Discussion forum with ID {DiscussionForumId} not found.", id);
                    return NotFound(new { Message = $"Discussion forum with ID {id} not found." });
                }

                var result = await _discussionForumService.DeleteDiscussionForumAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Failed to delete discussion forum with ID {DiscussionForumId}.", id);
                    return NotFound(new { Message = $"Failed to delete discussion forum with ID {id}." });
                }

                _logger.LogInformation("Successfully deleted discussion forum with ID {DiscussionForumId}.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting discussion forum with ID {DiscussionForumId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
