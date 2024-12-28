using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMS.Application.Services
{
    /// <summary>
    /// Service class for managing user-role associations.
    /// Provides methods for retrieving, assigning, and removing user roles with caching support.
    /// </summary>
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<UserRoleService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRoleService"/> class.
        /// </summary>
        /// <param name="userRoleRepository">The repository for user-role associations.</param>
        /// <param name="roleRepository">The repository for roles.</param>
        /// <param name="cache">The distributed cache for caching role data.</param>
        /// <param name="logger">The logger for logging user-role service operations.</param>
        public UserRoleService(IUserRoleRepository userRoleRepository, IRoleRepository roleRepository, IDistributedCache cache, ILogger<UserRoleService> logger)
        {
            _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all user roles from the repository with caching.
        /// Logs the caching process and any errors encountered.
        /// </summary>
        /// <returns>A list of <see cref="UserRoleDTO"/> objects.</returns>
        public async Task<IEnumerable<UserRoleDTO>> GetAllUserRolesAsync()
        {
            _logger.LogInformation("Fetching all user roles.");

            try
            {
                var cacheKey = "all_user_roles";
                // Try to get user roles from the cache
                var cachedUserRoles = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedUserRoles))
                {
                    _logger.LogInformation("Returning cached user roles.");
                    // Deserialize and return cached data
                    return JsonSerializer.Deserialize<IEnumerable<UserRoleDTO>>(cachedUserRoles);
                }

                // Fetch user roles from the database
                var userRoles = await _userRoleRepository.GetAllAsync();
                if (userRoles == null || !userRoles.Any())
                {
                    _logger.LogInformation("No user roles found in the database.");
                    // Return an empty list if no user roles are found
                    return Enumerable.Empty<UserRoleDTO>();
                }

                // Convert domain entities to DTOs
                var userRoleDtos = userRoles.Select(userRole => new UserRoleDTO
                {
                    UserId = userRole.UserId,
                    RoleId = userRole.RoleId,
                    RoleName = userRole.Role.Name
                }).ToList();

                _logger.LogInformation("Caching retrieved user roles.");
                // Cache the data with an absolute expiration
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)); // Cache expiration
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userRoleDtos), cacheOptions);

                return userRoleDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all user roles.");
                // Rethrow the exception after logging
                throw;
            }
        }

        /// <summary>
        /// Gets a specific user role based on user ID and role ID.
        /// Logs the caching process and any errors encountered.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="roleId">The role ID.</param>
        /// <returns>A <see cref="UserRoleDTO"/> object or null if not found.</returns>
        public async Task<UserRoleDTO> GetUserRoleAsync(int userId, int roleId)
        {
            _logger.LogInformation($"Fetching user role for userId: {userId}, roleId: {roleId}.");

            try
            {
                var cacheKey = $"user_role_{userId}_{roleId}";
                // Try to get user role from the cache
                var cachedUserRole = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedUserRole))
                {
                    _logger.LogInformation("Returning cached user role.");
                    // Deserialize and return cached data
                    return JsonSerializer.Deserialize<UserRoleDTO>(cachedUserRole);
                }

                // Fetch the user role from the database
                var userRole = await _userRoleRepository.GetByIdAsync(userId, roleId);
                if (userRole == null)
                {
                    _logger.LogWarning($"UserRole not found for userId: {userId}, roleId: {roleId}.");
                    // Return null if the user role is not found
                    return null;
                }

                // Convert domain entity to DTO
                var userRoleDto = new UserRoleDTO
                {
                    UserId = userRole.UserId,
                    RoleId = userRole.RoleId,
                    RoleName = userRole.Role.Name
                };

                _logger.LogInformation("Caching retrieved user role.");
                // Cache the data with an absolute expiration
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)); // Cache expiration
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userRoleDto), cacheOptions);

                return userRoleDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching user role for userId: {userId}, roleId: {roleId}.");
                // Rethrow the exception after logging
                throw;
            }
        }

        /// <summary>
        /// Assigns a role to a user and updates the cache.
        /// Logs the assignment process and any errors encountered.
        /// </summary>
        /// <param name="createUserRoleDto">The <see cref="CreateUserRoleDTO"/> object containing user ID and role ID.</param>
        public async Task AssignUserRoleAsync(CreateUserRoleDTO createUserRoleDto)
        {
            _logger.LogInformation($"Assigning roleId: {createUserRoleDto.RoleId} to userId: {createUserRoleDto.UserId}.");

            try
            {
                // Create a new UserRole entity
                var userRole = new UserRole
                {
                    UserId = createUserRoleDto.UserId,
                    RoleId = createUserRoleDto.RoleId
                };

                // Add the new user role to the repository
                await _userRoleRepository.AddAsync(userRole);
                _logger.LogInformation($"Successfully assigned roleId: {createUserRoleDto.RoleId} to userId: {createUserRoleDto.UserId}.");

                // Invalidate the cache for the specific user role
                var cacheKey = $"user_role_{createUserRoleDto.UserId}_{createUserRoleDto.RoleId}";
                await _cache.RemoveAsync(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while assigning roleId: {createUserRoleDto.RoleId} to userId: {createUserRoleDto.UserId}.");
                // Rethrow the exception after logging
                throw;
            }
        }

        /// <summary>
        /// Removes a role from a user and updates the cache.
        /// Logs the removal process and any errors encountered.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="roleId">The role ID.</param>
        public async Task RemoveUserRoleAsync(int userId, int roleId)
        {
            _logger.LogInformation($"Removing roleId: {roleId} from userId: {userId}.");

            try
            {
                // Remove the user role from the repository
                await _userRoleRepository.DeleteAsync(userId, roleId);
                _logger.LogInformation($"Successfully removed roleId: {roleId} from userId: {userId}.");

                // Invalidate the cache for the specific user role
                var cacheKey = $"user_role_{userId}_{roleId}";
                await _cache.RemoveAsync(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while removing roleId: {roleId} from userId: {userId}.");
                // Rethrow the exception after logging
                throw;
            }
        }
    }
}
