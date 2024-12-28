using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LMS.API.Controllers
{
    [Authorize(Roles = "Admin, Instructor")]
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("GlobalRateLimitPolicy")] // Apply the rate limiting policy to the entire controller
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ILogger<LessonsController> _logger;

        public LessonsController(ILessonService lessonService, ILogger<LessonsController> logger)
        {
            _lessonService = lessonService ?? throw new ArgumentNullException(nameof(lessonService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all lessons.
        /// </summary>
        /// <returns>A list of LessonDTO objects.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LessonDTO>>> GetLessons()
        {
            _logger.LogInformation("Initiating request to retrieve all lessons.");

            try
            {
                var lessons = await _lessonService.GetAllLessonsAsync();
                if (lessons == null || !lessons.Any())
                {
                    _logger.LogInformation("No lessons found in the database.");
                    return NotFound("No lessons found.");
                }

                _logger.LogInformation("Successfully retrieved all lessons.");
                return Ok(lessons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all lessons.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Retrieves a specific lesson by ID.
        /// </summary>
        /// <param name="id">The ID of the lesson to retrieve.</param>
        /// <returns>A LessonDTO object, or a NotFound status if the lesson does not exist.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<LessonDTO>> GetLesson(int id)
        {
            _logger.LogInformation("Initiating request to retrieve lesson with ID: {LessonId}.", id);

            try
            {
                var lesson = await _lessonService.GetLessonByIdAsync(id);
                if (lesson == null)
                {
                    _logger.LogWarning("Lesson with ID: {LessonId} not found.", id);
                    return NotFound($"Lesson with ID: {id} not found.");
                }

                _logger.LogInformation("Successfully retrieved lesson with ID: {LessonId}.", id);
                return Ok(lesson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving lesson with ID: {LessonId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Creates a new lesson.
        /// </summary>
        /// <param name="createLessonDto">The DTO containing the details of the lesson to create.</param>
        /// <returns>A newly created lesson, or a BadRequest status if the model state is invalid.</returns>
        [HttpPost]
        public async Task<ActionResult> CreateLesson([FromBody] CreateLessonDTO createLessonDto)
        {
            _logger.LogInformation("Initiating request to create a new lesson.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for the CreateLessonDTO object.");
                return BadRequest(ModelState);
            }

            try
            {
                var lessonId = await _lessonService.CreateLessonAsync(createLessonDto);
                _logger.LogInformation("Successfully created lesson with ID: {LessonId}.", lessonId);
                return CreatedAtAction(nameof(GetLesson), new { id = lessonId }, createLessonDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new lesson.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Updates an existing lesson.
        /// </summary>
        /// <param name="id">The ID of the lesson to update.</param>
        /// <param name="updateLessonDto">The DTO containing updated lesson details.</param>
        /// <returns>No content, or a BadRequest status if the model state is invalid.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromBody] UpdateLessonDTO updateLessonDto)
        {
            _logger.LogInformation("Initiating request to update lesson with ID: {LessonId}.", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for the UpdateLessonDTO object.");
                return BadRequest(ModelState);
            }

            try
            {
                var existingLesson = await _lessonService.GetLessonByIdAsync(id);
                if (existingLesson == null)
                {
                    _logger.LogWarning("Lesson with ID: {LessonId} not found.", id);
                    return NotFound($"Lesson with ID: {id} not found.");
                }

                await _lessonService.UpdateLessonAsync(id, updateLessonDto);
                _logger.LogInformation("Successfully updated lesson with ID: {LessonId}.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating lesson with ID: {LessonId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Deletes a lesson.
        /// </summary>
        /// <param name="id">The ID of the lesson to delete.</param>
        /// <returns>No content, or a NotFound status if the lesson does not exist.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            _logger.LogInformation("Initiating request to delete lesson with ID: {LessonId}.", id);

            try
            {
                var existingLesson = await _lessonService.GetLessonByIdAsync(id);
                if (existingLesson == null)
                {
                    _logger.LogWarning("Lesson with ID: {LessonId} not found.", id);
                    return NotFound($"Lesson with ID: {id} not found.");
                }

                await _lessonService.DeleteLessonAsync(id);
                _logger.LogInformation("Successfully deleted lesson with ID: {LessonId}.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting lesson with ID: {LessonId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
