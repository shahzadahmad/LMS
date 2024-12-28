using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Applying authorization to all actions in this controller
    public class ForumPostsController : ControllerBase
    {
        private readonly IForumPostService _forumPostService;
        private readonly ILogger<ForumPostsController> _logger;

        public ForumPostsController(IForumPostService forumPostService, ILogger<ForumPostsController> logger)
        {
            _forumPostService = forumPostService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a forum post by its ID.
        /// </summary>
        /// <param name="postId">The ID of the forum post to retrieve.</param>
        /// <returns>An ActionResult containing the forum post.</returns>
        [HttpGet("{postId}")]
        [Authorize(Roles = "Admin,Instructor,Student")] // Specifying roles allowed to access this endpoint
        public async Task<IActionResult> GetForumPostById(int postId)
        {
            _logger.LogInformation("GET request received for forum post with ID {PostId}", postId);
            try
            {
                var forumPost = await _forumPostService.GetForumPostByIdAsync(postId);
                if (forumPost == null)
                {
                    _logger.LogWarning("Forum post with ID {PostId} not found", postId);
                    return NotFound(); // Return 404 if the forum post is not found
                }

                _logger.LogInformation("Returning forum post with ID {PostId}", postId);
                return Ok(forumPost); // Return 200 with the forum post
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving forum post with ID {PostId}", postId);
                return StatusCode(500, "Internal server error"); // Return 500 in case of an unexpected error
            }
        }

        /// <summary>
        /// Retrieves all forum posts.
        /// </summary>
        /// <returns>An ActionResult containing the list of all forum posts.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> GetAllForumPosts()
        {
            _logger.LogInformation("GET request received for all forum posts");
            try
            {
                var forumPosts = await _forumPostService.GetAllForumPostsAsync();
                if (forumPosts == null || !forumPosts.Any())
                {
                    _logger.LogInformation("No forum posts found");
                    return NoContent(); // Return 204 if no forum posts are found
                }

                _logger.LogInformation("Returning all forum posts");
                return Ok(forumPosts); // Return 200 with the list of forum posts
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all forum posts.");
                return StatusCode(500, "Internal server error"); // Return 500 in case of an unexpected error
            }
        }

        /// <summary>
        /// Creates a new forum post.
        /// </summary>
        /// <param name="createForumPostDto">The DTO containing the forum post details.</param>
        /// <returns>An ActionResult containing the ID of the newly created forum post.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> CreateForumPost(CreateForumPostDTO createForumPostDto)
        {
            _logger.LogInformation("POST request received to create a new forum post");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid data received for creating a forum post");
                return BadRequest(ModelState); // Return 400 if the data is invalid
            }

            try
            {
                var postId = await _forumPostService.CreateForumPostAsync(createForumPostDto);
                _logger.LogInformation("Forum post with ID {PostId} created successfully", postId);

                return CreatedAtAction(nameof(GetForumPostById), new { postId }, createForumPostDto); // Return 201 with the location of the new resource
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new forum post.");
                return StatusCode(500, "Internal server error"); // Return 500 in case of an unexpected error
            }
        }

        /// <summary>
        /// Updates an existing forum post.
        /// </summary>
        /// <param name="postId">The ID of the forum post to update.</param>
        /// <param name="updateForumPostDto">The data transfer object containing the updated information for the forum post.</param>
        /// <returns>An ActionResult indicating the result of the update operation.</returns>
        [HttpPut("{postId}")]
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> UpdateForumPost(int postId, [FromBody] UpdateForumPostDTO updateForumPostDto)
        {
            _logger.LogInformation("PUT request received to update forum post with ID {PostId}", postId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid data received for updating forum post with ID {PostId}", postId);
                return BadRequest(ModelState); // Return 400 if the model state is invalid
            }

            try
            {
                var result = await _forumPostService.UpdateForumPostAsync(postId, updateForumPostDto);
                if (!result)
                {
                    _logger.LogWarning("Forum post with ID {PostId} not found or update failed", postId);
                    return NotFound(); // Return 404 if the post is not found or the update fails
                }

                _logger.LogInformation("Forum post with ID {PostId} updated successfully", postId);
                return NoContent(); // Return 204 to indicate successful update with no content
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating forum post with ID {PostId}", postId);
                return StatusCode(500, "An internal server error occurred while processing your request."); // Return 500 in case of an exception
            }
        }

        /// <summary>
        /// Deletes an existing forum post by its ID.
        /// </summary>
        /// <param name="postId">The ID of the forum post to delete.</param>
        /// <returns>An ActionResult indicating the result of the deletion operation.</returns>
        [HttpDelete("{postId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> DeleteForumPost(int postId)
        {
            _logger.LogInformation("DELETE request received for forum post with ID {PostId}", postId);

            try
            {
                var result = await _forumPostService.DeleteForumPostAsync(postId);
                if (!result)
                {
                    _logger.LogWarning("Forum post with ID {PostId} not found or deletion failed", postId);
                    return NotFound(); // Return 404 if the post is not found or the deletion fails
                }

                _logger.LogInformation("Forum post with ID {PostId} deleted successfully", postId);
                return NoContent(); // Return 204 to indicate successful deletion with no content
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting forum post with ID {PostId}", postId);
                return StatusCode(500, "An internal server error occurred while processing your request."); // Return 500 in case of an exception
            }
        }
    }
}
