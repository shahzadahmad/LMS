using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("GlobalRateLimitPolicy")] // Apply the rate limiting policy to the entire controller
    public class UserRolesController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<UserRolesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRolesController"/> class.
        /// </summary>
        /// <param name="userRoleService">The user role service for managing user-role operations.</param>
        /// <param name="logger">The logger for logging controller operations.</param>
        public UserRolesController(IUserRoleService userRoleService, ILogger<UserRolesController> logger)
        {
            _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all user roles.
        /// </summary>
        /// <returns>A list of <see cref="UserRoleDTO"/> objects.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserRoleDTO>>> GetUserRoles()
        {
            _logger.LogInformation("Fetching all user roles.");
            try
            {
                // Call service method to get all user roles
                var userRoles = await _userRoleService.GetAllUserRolesAsync();

                // Return the user roles with a 200 OK response
                return Ok(userRoles);
            }
            catch (Exception ex)
            {
                // Log the exception with error details
                _logger.LogError(ex, "An error occurred while fetching all user roles.");
                // Return a 500 Internal Server Error response with the error message
                return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific user role.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="roleId">The role ID.</param>
        /// <returns>A <see cref="UserRoleDTO"/> object if found; otherwise, a 404 Not Found response.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{userId}/{roleId}")]
        public async Task<ActionResult<UserRoleDTO>> GetUserRole(int userId, int roleId)
        {
            _logger.LogInformation($"Fetching user role for userId: {userId}, roleId: {roleId}.");
            try
            {
                // Call service method to get user role by ID
                var userRole = await _userRoleService.GetUserRoleAsync(userId, roleId);

                if (userRole == null)
                {
                    // Log a warning if the user role is not found
                    _logger.LogWarning($"UserRole not found for userId: {userId}, roleId: {roleId}.");
                    // Return a 404 Not Found response
                    return NotFound();
                }

                // Return the user role with a 200 OK response
                return Ok(userRole);
            }
            catch (Exception ex)
            {
                // Log the exception with error details
                _logger.LogError(ex, $"An error occurred while fetching user role for userId: {userId}, roleId: {roleId}.");
                // Return a 500 Internal Server Error response with the error message
                return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
            }
        }

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        /// <param name="createUserRoleDto">The <see cref="CreateUserRoleDTO"/> object containing user ID and role ID.</param>
        /// <returns>An IActionResult indicating the outcome of the operation.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignUserRole([FromBody] CreateUserRoleDTO createUserRoleDto)
        {
            _logger.LogInformation($"Assigning roleId: {createUserRoleDto.RoleId} to userId: {createUserRoleDto.UserId}.");
            try
            {
                // Call service method to assign a role to the user
                await _userRoleService.AssignUserRoleAsync(createUserRoleDto);

                // Return a success message with a 200 OK response
                return Ok(new { message = "Role assigned successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception with error details
                _logger.LogError(ex, $"An error occurred while assigning roleId: {createUserRoleDto.RoleId} to userId: {createUserRoleDto.UserId}.");
                // Return a 500 Internal Server Error response with the error message
                return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
            }
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="roleId">The role ID.</param>
        /// <returns>An IActionResult indicating the outcome of the operation.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("remove/{userId}/{roleId}")]
        public async Task<IActionResult> RemoveUserRole(int userId, int roleId)
        {
            _logger.LogInformation($"Removing roleId: {roleId} from userId: {userId}.");
            try
            {
                // Call service method to remove the role from the user
                await _userRoleService.RemoveUserRoleAsync(userId, roleId);

                // Return a success message with a 200 OK response
                return Ok(new { message = "Role removed successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception with error details
                _logger.LogError(ex, $"An error occurred while removing roleId: {roleId} from userId: {userId}.");
                // Return a 500 Internal Server Error response with the error message
                return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
            }
        }
    }
}
