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
    public class AssessmentsController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<AssessmentsController> _logger;

        public AssessmentsController(IAssessmentService assessmentService, ILogger<AssessmentsController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all assessments.
        /// </summary>
        /// <returns>A list of AssessmentDTO objects.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssessmentDTO>>> GetAllAssessments()
        {
            _logger.LogInformation("Initiating request to fetch all assessments");

            try
            {
                var assessments = await _assessmentService.GetAllAssessmentsAsync();

                if (assessments == null || !assessments.Any())
                {
                    _logger.LogWarning("No assessments found");
                    return NotFound(new { Message = "No assessments available." });
                }

                _logger.LogInformation("Successfully fetched {AssessmentCount} assessments", assessments.Count());
                return Ok(assessments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all assessments");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a specific assessment by ID.
        /// </summary>
        /// <param name="id">The ID of the assessment.</param>
        /// <returns>The requested AssessmentDTO object.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<AssessmentDTO>> GetAssessmentById(int id)
        {
            _logger.LogInformation("Initiating request to fetch assessment with ID {AssessmentId}", id);

            try
            {
                var assessment = await _assessmentService.GetAssessmentByIdAsync(id);

                if (assessment == null)
                {
                    _logger.LogWarning("Assessment with ID {AssessmentId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully fetched assessment with ID {AssessmentId}", id);
                return Ok(assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching assessment with ID {AssessmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new assessment.
        /// </summary>
        /// <param name="createAssessmentDto">The details of the assessment to be created.</param>
        /// <returns>The ID of the newly created assessment.</returns>
        [HttpPost]
        public async Task<ActionResult> CreateAssessment([FromBody] CreateAssessmentDTO createAssessmentDto)
        {
            _logger.LogInformation("Initiating request to create a new assessment");

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateAssessmentDTO");
                    return BadRequest(ModelState);
                }

                var assessmentId = await _assessmentService.CreateAssessmentAsync(createAssessmentDto);

                _logger.LogInformation("Successfully created assessment with ID {AssessmentId}", assessmentId);
                return CreatedAtAction(nameof(GetAssessmentById), new { id = assessmentId }, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new assessment");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an existing assessment.
        /// </summary>
        /// <param name="id">The ID of the assessment to be updated.</param>
        /// <param name="updateAssessmentDto">The updated details of the assessment.</param>
        /// <returns>NoContent on success or NotFound if the assessment doesn't exist.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssessment(int id, [FromBody] UpdateAssessmentDTO updateAssessmentDto)
        {
            _logger.LogInformation("Initiating request to update assessment with ID {AssessmentId}", id);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateAssessmentDTO");
                    return BadRequest(ModelState);
                }

                var existingAssessment = await _assessmentService.GetAssessmentByIdAsync(id);

                if (existingAssessment == null)
                {
                    _logger.LogWarning("Assessment with ID {AssessmentId} not found", id);
                    return NotFound();
                }

                var result = await _assessmentService.UpdateAssessmentAsync(id, updateAssessmentDto);

                if (!result)
                {
                    _logger.LogWarning("Failed to update assessment with ID {AssessmentId}", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully updated assessment with ID {AssessmentId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating assessment with ID {AssessmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete an assessment by ID.
        /// </summary>
        /// <param name="id">The ID of the assessment to be deleted.</param>
        /// <returns>NoContent on success or NotFound if the assessment doesn't exist.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessment(int id)
        {
            _logger.LogInformation("Initiating request to delete assessment with ID {AssessmentId}", id);

            try
            {
                var existingAssessment = await _assessmentService.GetAssessmentByIdAsync(id);

                if (existingAssessment == null)
                {
                    _logger.LogWarning("Assessment with ID {AssessmentId} not found", id);
                    return NotFound();
                }

                var result = await _assessmentService.DeleteAssessmentAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Failed to delete assessment with ID {AssessmentId}", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully deleted assessment with ID {AssessmentId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting assessment with ID {AssessmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
