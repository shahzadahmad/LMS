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
    public class ModulesController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        private readonly ILogger<ModulesController> _logger;

        public ModulesController(IModuleService moduleService, ILogger<ModulesController> logger)
        {
            _moduleService = moduleService ?? throw new ArgumentNullException(nameof(moduleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all modules.
        /// Logs the retrieval process and handles any errors.
        /// </summary>
        /// <returns>A list of <see cref="ModuleDTO"/>s.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModuleDTO>>> GetModules()
        {
            try
            {
                _logger.LogInformation("Request received to get all modules.");

                var modules = await _moduleService.GetAllModulesAsync();
                if (modules == null)
                {
                    _logger.LogWarning("No modules found.");
                    return NotFound("No modules found.");
                }

                _logger.LogInformation("Successfully retrieved all modules.");
                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all modules.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Retrieves a specific module by its ID.
        /// Logs the retrieval process and handles any errors.
        /// </summary>
        /// <param name="id">The ID of the module.</param>
        /// <returns>A <see cref="ModuleDTO"/> or a NotFound result.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ModuleDTO>> GetModule(int id)
        {
            try
            {
                _logger.LogInformation("Request received to get module with ID {ModuleId}.", id);

                var module = await _moduleService.GetModuleByIdAsync(id);
                if (module == null)
                {
                    _logger.LogWarning("Module with ID {ModuleId} not found.", id);
                    return NotFound($"Module with ID {id} not found.");
                }

                _logger.LogInformation("Successfully retrieved module with ID {ModuleId}.", id);
                return Ok(module);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving module with ID {ModuleId}.", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Creates a new module.
        /// Logs the creation process, validates the input, and handles any errors.
        /// </summary>
        /// <param name="createModuleDto">The DTO containing module creation details.</param>
        /// <returns>A CreatedAtAction result.</returns>
        [HttpPost]
        public async Task<ActionResult> CreateModule([FromBody] CreateModuleDTO createModuleDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for module creation: {ModelState}.", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Request received to create a new module.");

                var moduleId = await _moduleService.CreateModuleAsync(createModuleDto);
                _logger.LogInformation("Module with ID {ModuleId} created successfully.", moduleId);

                return CreatedAtAction(nameof(GetModule), new { id = moduleId }, createModuleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new module.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Updates an existing module by its ID.
        /// Logs the update process, validates the input, and handles any errors.
        /// </summary>
        /// <param name="id">The ID of the module to update.</param>
        /// <param name="updateModuleDto">The DTO containing update details.</param>
        /// <returns>A NoContent result or a NotFound result.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModule(int id, [FromBody] UpdateModuleDTO updateModuleDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for module update: {ModelState}.", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Request received to update module with ID {ModuleId}.", id);

                var module = await _moduleService.GetModuleByIdAsync(id);
                if (module == null)
                {
                    _logger.LogWarning("Module with ID {ModuleId} not found for update.", id);
                    return NotFound($"Module with ID {id} not found.");
                }

                await _moduleService.UpdateModuleAsync(id, updateModuleDto);
                _logger.LogInformation("Module with ID {ModuleId} updated successfully.", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating module with ID {ModuleId}.", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Deletes a module by its ID.
        /// Logs the deletion process and handles any errors.
        /// </summary>
        /// <param name="id">The ID of the module to delete.</param>
        /// <returns>A NoContent result or a NotFound result.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModule(int id)
        {
            try
            {
                _logger.LogInformation("Request received to delete module with ID {ModuleId}.", id);

                var module = await _moduleService.GetModuleByIdAsync(id);
                if (module == null)
                {
                    _logger.LogWarning("Module with ID {ModuleId} not found for deletion.", id);
                    return NotFound($"Module with ID {id} not found.");
                }

                await _moduleService.DeleteModuleAsync(id);
                _logger.LogInformation("Module with ID {ModuleId} deleted successfully.", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting module with ID {ModuleId}.", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
