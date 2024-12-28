using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace LMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("GlobalRateLimitPolicy")] // Apply the rate limiting policy to the entire controller
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of <see cref="UserDTO"/> objects representing all users.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            _logger.LogInformation("Getting all users.");
            try
            {
                var users = await _userService.GetAllUsersAsync();
                _logger.LogInformation("Successfully retrieved all users.");
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all users.");
                return StatusCode(500, new { message = "An error occurred while retrieving users." });
            }
        }

        /// <summary>
        /// Retrieves a user by its ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>A <see cref="UserDTO"/> object representing the user, or <c>404 Not Found</c> if not found.</returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            _logger.LogInformation($"Getting user with ID {id}.");
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Assuming user ID is in claims
                if (id.ToString() != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid(); // Users can only get their own details or admin can get any user's details
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found.");
                    return NotFound();
                }
                _logger.LogInformation($"Successfully retrieved user with ID {id}.");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting user with ID {id}.");
                return StatusCode(500, new { message = "An error occurred while retrieving the user." });
            }
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// </summary>
        /// <param name="loginDto">The login details.</param>
        /// <returns>A JWT token if authentication is successful, or <c>401 Unauthorized</c> if failed.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            _logger.LogInformation("User login attempt.");
            try
            {
                var user = await _userService.AuthenticateUserAsync(loginDto.Username, loginDto.Password);
                if (user == null)
                {
                    _logger.LogWarning("Invalid login attempt.");
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                var token = _userService.GenerateJwtToken(user);
                _logger.LogInformation("User successfully logged in.");
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user login.");
                return StatusCode(500, new { message = "An error occurred during login." });
            }
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="createUserDto">The user details.</param>
        /// <returns>A success response with the new user's ID.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] CreateUserDTO createUserDto)
        {
            _logger.LogInformation("User registration attempt.");
            try
            {
                var userId = await _userService.CreateUserAsync(createUserDto);
                _logger.LogInformation($"User registered successfully with ID {userId}.");
                return Ok(new { UserId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user registration.");
                return StatusCode(500, new { message = "An error occurred while registering the user." });
            }
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="updateUserDto">The updated user details.</param>
        /// <returns>A success response if the update is successful.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDto)
        {
            _logger.LogInformation($"Updating user with ID {id}.");
            try
            {
                await _userService.UpdateUserAsync(id, updateUserDto);
                _logger.LogInformation($"User with ID {id} updated successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user with ID {id}.");
                return StatusCode(500, new { message = "An error occurred while updating the user." });
            }
        }

        /// <summary>
        /// Deletes a user by its ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>A success response if the deletion is successful.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation($"Deleting user with ID {id}.");
            try
            {
                await _userService.DeleteUserAsync(id);
                _logger.LogInformation($"User with ID {id} deleted successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting user with ID {id}.");
                return StatusCode(500, new { message = "An error occurred while deleting the user." });
            }
        }

        /// <summary>
        /// Assigns roles to a user.
        /// </summary>
        /// <param name="userId">The ID of the user to assign roles to.</param>
        /// <param name="assignRolesDto">The role assignment details.</param>
        /// <returns>A success response if roles are assigned successfully.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/assign-roles")]
        public async Task<IActionResult> AssignRoles(int userId, [FromBody] AssignRolesDTO assignRolesDto)
        {
            _logger.LogInformation($"Assigning roles to user with ID {userId}.");
            try
            {
                await _userService.AssignRolesAsync(userId, assignRolesDto.RoleIds);
                _logger.LogInformation($"Roles assigned successfully to user with ID {userId}.");
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Invalid roles assignment for user with ID {userId}.");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while assigning roles to user with ID {userId}.");
                return StatusCode(500, new { message = "An error occurred while assigning roles." });
            }
        }
    }
}
