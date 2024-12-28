using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LMS.Api.Controllers
{
    [Authorize(Roles = "Admin, Instructor")] // Allow Admin and Instructor roles
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("GlobalRateLimitPolicy")] // Apply rate limiting to the entire controller
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ILogger<AnnouncementsController> _logger;

        public AnnouncementsController(IAnnouncementService announcementService, ILogger<AnnouncementsController> logger)
        {
            _announcementService = announcementService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all announcements.
        /// </summary>
        /// <returns>A list of announcements.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAnnouncements()
        {
            _logger.LogInformation("Initiating retrieval of all announcements");

            try
            {
                var announcements = await _announcementService.GetAllAnnouncementsAsync();

                if (announcements == null || !announcements.Any())
                {
                    _logger.LogWarning("No announcements found");
                    return NotFound(new { Message = "No announcements available." });
                }

                _logger.LogInformation("Successfully retrieved {AnnouncementCount} announcements", announcements.Count());
                return Ok(announcements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all announcements");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves a specific announcement by its ID.
        /// </summary>
        /// <param name="id">The ID of the announcement.</param>
        /// <returns>The requested announcement.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnnouncementById(int id)
        {
            _logger.LogInformation("Initiating retrieval of announcement with ID {AnnouncementId}", id);

            try
            {
                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);

                if (announcement == null)
                {
                    _logger.LogWarning("Announcement with ID {AnnouncementId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved announcement with ID {AnnouncementId}", id);
                return Ok(announcement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving announcement with ID {AnnouncementId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new announcement.
        /// </summary>
        /// <param name="createAnnouncementDTO">The data to create the announcement.</param>
        /// <returns>The newly created announcement's ID.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementDTO createAnnouncementDTO)
        {
            _logger.LogInformation("Initiating creation of a new announcement");

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateAnnouncementDTO");
                    return BadRequest(ModelState);
                }

                var announcementId = await _announcementService.CreateAnnouncementAsync(createAnnouncementDTO);
                _logger.LogInformation("Successfully created announcement with ID {AnnouncementId}", announcementId);
                return CreatedAtAction(nameof(GetAnnouncementById), new { id = announcementId }, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new announcement");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing announcement.
        /// </summary>
        /// <param name="id">The ID of the announcement to update.</param>
        /// <param name="updateAnnouncementDto">The updated announcement data.</param>
        /// <returns>NoContent if successful.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnnouncement(int id, [FromBody] UpdateAnnouncementDTO updateAnnouncementDto)
        {
            _logger.LogInformation("Initiating update of announcement with ID {AnnouncementId}", id);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateAnnouncementDTO");
                    return BadRequest(ModelState);
                }

                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                if (announcement == null)
                {
                    _logger.LogWarning("Announcement with ID {AnnouncementId} not found", id);
                    return NotFound();
                }

                var result = await _announcementService.UpdateAnnouncementAsync(updateAnnouncementDto);
                if (!result)
                {
                    _logger.LogWarning("Announcement with ID {AnnouncementId} not found after update attempt", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully updated announcement with ID {AnnouncementId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating announcement with ID {AnnouncementId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes an existing announcement.
        /// </summary>
        /// <param name="id">The ID of the announcement to delete.</param>
        /// <returns>NoContent if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            _logger.LogInformation("Initiating deletion of announcement with ID {AnnouncementId}", id);

            try
            {
                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                if (announcement == null)
                {
                    _logger.LogWarning("Announcement with ID {AnnouncementId} not found", id);
                    return NotFound();
                }

                var result = await _announcementService.DeleteAnnouncementAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Announcement with ID {AnnouncementId} not found after deletion attempt", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully deleted announcement with ID {AnnouncementId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting announcement with ID {AnnouncementId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
