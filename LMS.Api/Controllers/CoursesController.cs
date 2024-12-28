using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LMS.API.Controllers
{
    [Authorize(Roles = "Admin, Instructor")] // Apply authorization for Admin and Instructor roles
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("GlobalRateLimitPolicy")] // Apply the rate limiting policy to the entire controller

    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ICourseService courseService, ILogger<CoursesController> logger)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET: api/courses
        /// Retrieves all courses.
        /// </summary>
        /// <returns>An ActionResult containing a list of CourseDTO objects.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetCourses()
        {
            _logger.LogInformation("Initiating request to fetch all courses.");

            try
            {
                // Fetch all courses from the service
                var courses = await _courseService.GetAllCoursesAsync();

                // Check if courses were found
                if (courses == null || !courses.Any())
                {
                    _logger.LogInformation("No courses found.");
                    return NotFound(new { Message = "No courses available." });
                }

                _logger.LogInformation("Successfully fetched {CourseCount} courses.", courses.Count());
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching courses.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// GET: api/courses/{id}
        /// Retrieves a specific course by ID.
        /// </summary>
        /// <param name="id">The ID of the course to retrieve.</param>
        /// <returns>An ActionResult containing a CourseDTO object.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDTO>> GetCourseById(int id)
        {
            _logger.LogInformation("Initiating request to fetch course with ID {CourseId}.", id);

            try
            {
                // Fetch the specific course from the service
                var course = await _courseService.GetCourseByIdAsync(id);

                // Check if the course was found
                if (course == null)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found.", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully fetched course with ID {CourseId}.", id);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching course with ID {CourseId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// POST: api/courses
        /// Creates a new course.
        /// </summary>
        /// <param name="createCourseDto">The details of the course to create.</param>
        /// <returns>An ActionResult indicating the result of the creation operation.</returns>
        [Authorize(Roles = "Admin")] // Only Admins are authorized to create a new course
        [HttpPost]
        public async Task<ActionResult> CreateCourse([FromBody] CreateCourseDTO createCourseDto)
        {
            _logger.LogInformation("Initiating request to create a new course.");

            try
            {
                // Validate the input model
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateCourseDTO object.");
                    return BadRequest(ModelState);
                }

                // Create the new course
                var courseId = await _courseService.CreateCourseAsync(createCourseDto);
                _logger.LogInformation("Successfully created course with ID {CourseId}.", courseId);

                // Return a response indicating the creation was successful
                return CreatedAtAction(nameof(GetCourseById), new { id = courseId }, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new course.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// PUT: api/courses/{id}
        /// Updates an existing course by ID.
        /// </summary>
        /// <param name="id">The ID of the course to update.</param>
        /// <param name="updateCourseDto">The updated details of the course.</param>
        /// <returns>An IActionResult indicating the result of the update operation.</returns>
        [Authorize(Roles = "Admin")] // Only Admins are authorized to update a course
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDTO updateCourseDto)
        {
            _logger.LogInformation("Initiating request to update course with ID {CourseId}.", id);

            try
            {
                // Validate the input model
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateCourseDTO object.");
                    return BadRequest(ModelState);
                }

                // Update the course
                var result = await _courseService.UpdateCourseAsync(id, updateCourseDto);

                // Check if the update was successful
                if (!result)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found for update.", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully updated course with ID {CourseId}.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating course with ID {CourseId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// DELETE: api/courses/{id}
        /// Deletes a course by ID.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
        /// <returns>An IActionResult indicating the result of the deletion operation.</returns>
        [Authorize(Roles = "Admin")] // Only Admins are authorized to delete a course
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            _logger.LogInformation("Initiating request to delete course with ID {CourseId}.", id);

            try
            {
                // Fetch the course to check if it exists
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found for deletion.", id);
                    return NotFound();
                }

                // Delete the course
                var result = await _courseService.DeleteCourseAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Course with ID {CourseId} could not be deleted.", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully deleted course with ID {CourseId}.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting course with ID {CourseId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
