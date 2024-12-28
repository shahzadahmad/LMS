using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

/// <summary>
/// Controller class for managing roles in the LMS application.
/// Provides endpoints for retrieving, creating, updating, and deleting roles.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("GlobalRateLimitPolicy")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolesController"/> class.
    /// </summary>
    /// <param name="roleService">The service for managing roles.</param>
    /// <param name="logger">The logger for logging role-related operations.</param>
    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all roles from the system.
    /// Logs the process and results of fetching roles.
    /// </summary>
    /// <returns>An <see cref="ActionResult"/> containing a collection of <see cref="RoleDTO"/> objects.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDTO>>> GetRoles()
    {
        try
        {
            _logger.LogInformation("Fetching all roles.");
            var roles = await _roleService.GetAllRolesAsync();
            _logger.LogInformation("Roles retrieved successfully.");
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all roles.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retrieves a specific role by its ID.
    /// Logs the process and results of fetching the role.
    /// </summary>
    /// <param name="id">The ID of the role to retrieve.</param>
    /// <returns>An <see cref="ActionResult"/> containing the <see cref="RoleDTO"/> object or a not found response.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDTO>> GetRole(int id)
    {
        try
        {
            _logger.LogInformation($"Fetching role with ID {id}.");
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                _logger.LogWarning($"Role with ID {id} not found.");
                return NotFound();
            }
            _logger.LogInformation($"Role with ID {id} retrieved successfully.");
            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while fetching role with ID {id}.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new role in the system.
    /// Logs the process and results of creating the role.
    /// </summary>
    /// <param name="createRoleDto">The data transfer object containing the details of the role to create.</param>
    /// <returns>An <see cref="ActionResult"/> indicating the creation status and the ID of the new role.</returns>
    [HttpPost]
    public async Task<ActionResult> CreateRole([FromBody] CreateRoleDTO createRoleDto)
    {
        try
        {
            if (createRoleDto == null)
            {
                _logger.LogWarning("CreateRoleDto is null.");
                return BadRequest("Role data is required.");
            }

            _logger.LogInformation("Creating a new role.");
            var roleId = await _roleService.CreateRoleAsync(createRoleDto);
            _logger.LogInformation($"Role created successfully with ID {roleId}.");
            return CreatedAtAction(nameof(GetRole), new { id = roleId }, createRoleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating a new role.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates an existing role in the system.
    /// Logs the process and results of updating the role.
    /// </summary>
    /// <param name="id">The ID of the role to update.</param>
    /// <param name="updateRoleDto">The data transfer object containing the updated role details.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the update status.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDTO updateRoleDto)
    {
        try
        {
            if (updateRoleDto == null)
            {
                _logger.LogWarning("UpdateRoleDto is null.");
                return BadRequest("Role data is required.");
            }

            _logger.LogInformation($"Updating role with ID {id}.");
            await _roleService.UpdateRoleAsync(id, updateRoleDto);
            _logger.LogInformation($"Role with ID {id} updated successfully.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while updating role with ID {id}.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deletes a role from the system.
    /// Logs the process and results of deleting the role.
    /// </summary>
    /// <param name="id">The ID of the role to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the deletion status.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            _logger.LogInformation($"Deleting role with ID {id}.");
            await _roleService.DeleteRoleAsync(id);
            _logger.LogInformation($"Role with ID {id} deleted successfully.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while deleting role with ID {id}.");
            return StatusCode(500, "Internal server error");
        }
    }
}
