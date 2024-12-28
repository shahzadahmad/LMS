using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AnswersController : ControllerBase
    {
        private readonly IAnswerService _answerService;
        private readonly ILogger<AnswersController> _logger;

        public AnswersController(IAnswerService answerService, ILogger<AnswersController> logger)
        {
            _answerService = answerService ?? throw new ArgumentNullException(nameof(answerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all answers.
        /// </summary>
        /// <returns>A list of AnswerDTO objects.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnswerDTO>>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all answers.");
                var answers = await _answerService.GetAllAsync();
                return Ok(answers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all answers.");
                return StatusCode(500, "An error occurred while retrieving answers.");
            }
        }

        /// <summary>
        /// Retrieves a specific answer by its ID.
        /// </summary>
        /// <param name="id">The ID of the answer.</param>
        /// <returns>An AnswerDTO object.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<AnswerDTO>> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting answer with ID {AnswerId}.", id);
                var answer = await _answerService.GetByIdAsync(id);
                if (answer == null)
                {
                    _logger.LogWarning("Answer with ID {AnswerId} not found.", id);
                    return NotFound();
                }
                return Ok(answer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving answer with ID {AnswerId}.", id);
                return StatusCode(500, "An error occurred while retrieving the answer.");
            }
        }

        /// <summary>
        /// Adds a new answer.
        /// </summary>
        /// <param name="createAnswerDTO">The DTO containing answer data.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] CreateAnswerDTO createAnswerDTO)
        {
            try
            {
                if (createAnswerDTO == null)
                {
                    _logger.LogWarning("Received null CreateAnswerDTO.");
                    return BadRequest("Answer data is required.");
                }

                _logger.LogInformation("Adding new answer with QuestionId {QuestionId}.", createAnswerDTO.QuestionId);
                await _answerService.AddAsync(createAnswerDTO);
                return NoContent(); // 204 No Content for successful POST
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new answer.");
                return StatusCode(500, "An error occurred while adding the answer.");
            }
        }

        /// <summary>
        /// Updates an existing answer.
        /// </summary>
        /// <param name="updateAnswerDTO">The DTO containing updated answer data.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateAnswerDTO updateAnswerDTO)
        {
            try
            {
                if (updateAnswerDTO == null)
                {
                    _logger.LogWarning("Received null UpdateAnswerDTO.");
                    return BadRequest("Answer data is required.");
                }

                _logger.LogInformation("Updating answer with ID {AnswerId}.", updateAnswerDTO.AnswerId);
                await _answerService.UpdateAsync(updateAnswerDTO);
                return NoContent(); // 204 No Content for successful PUT
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating answer with ID {AnswerId}.", updateAnswerDTO.AnswerId);
                return StatusCode(500, "An error occurred while updating the answer.");
            }
        }

        /// <summary>
        /// Deletes an answer by its ID.
        /// </summary>
        /// <param name="id">The ID of the answer to be deleted.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting answer with ID {AnswerId}.", id);
                await _answerService.DeleteAsync(id);
                return NoContent(); // 204 No Content for successful DELETE
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting answer with ID {AnswerId}.", id);
                return StatusCode(500, "An error occurred while deleting the answer.");
            }
        }
    }
}