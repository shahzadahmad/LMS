using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Api.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing questions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly ILogger<QuestionsController> _logger;

        public QuestionsController(IQuestionService questionService, ILogger<QuestionsController> logger)
        {
            _questionService = questionService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a question by its ID.
        /// </summary>
        /// <param name="id">The ID of the question to retrieve.</param>
        /// <returns>An ActionResult containing the question DTO if found, or a NotFound result.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> GetQuestionByIdAsync(int id)
        {
            _logger.LogInformation("Getting question with ID {Id}", id);

            try
            {
                var questionDto = await _questionService.GetQuestionByIdAsync(id);
                if (questionDto == null)
                {
                    _logger.LogWarning("Question with ID {Id} not found", id);
                    return NotFound();
                }

                return Ok(questionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving question with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves all questions.
        /// </summary>
        /// <returns>An ActionResult containing a list of all question DTOs.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> GetAllQuestionsAsync()
        {
            _logger.LogInformation("Getting all questions");

            try
            {
                var questionsDto = await _questionService.GetAllQuestionsAsync();
                return Ok(questionsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all questions");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new question.
        /// </summary>
        /// <param name="createQuestionDto">The DTO containing the information of the question to create.</param>
        /// <returns>An ActionResult indicating the result of the creation operation.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CreateQuestionAsync([FromBody] CreateQuestionDTO createQuestionDto)
        {
            _logger.LogInformation("Creating a new question with Assessment ID {AssessmentId}", createQuestionDto.AssessmentId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for create question request");
                return BadRequest(ModelState);
            }

            try
            {
                await _questionService.AddAsync(createQuestionDto);
                return CreatedAtAction(nameof(GetQuestionByIdAsync), new { id = createQuestionDto.AssessmentId }, createQuestionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating question with Assessment ID {AssessmentId}", createQuestionDto.AssessmentId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing question.
        /// </summary>
        /// <param name="id">The ID of the question to update.</param>
        /// <param name="updateQuestionDto">The DTO containing the updated information for the question.</param>
        /// <returns>An ActionResult indicating the result of the update operation.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UpdateQuestionAsync(int id, [FromBody] UpdateQuestionDTO updateQuestionDto)
        {
            _logger.LogInformation("Updating question with ID {Id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for update question request");
                return BadRequest(ModelState);
            }

            try
            {
                updateQuestionDto.QuestionId = id;
                await _questionService.UpdateAsync(updateQuestionDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating question with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a question by its ID.
        /// </summary>
        /// <param name="id">The ID of the question to delete.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Restrict access to Admin role
        public async Task<IActionResult> DeleteQuestionAsync(int id)
        {
            _logger.LogInformation("Deleting question with ID {Id}", id);

            try
            {
                await _questionService.DeleteAsync(id);
                _logger.LogInformation("Question with ID {Id} deleted successfully.", id);

                // Return NoContent status to indicate successful deletion
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting question with ID {Id}", id);
                // Return a 500 Internal Server Error response in case of an exception
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}

