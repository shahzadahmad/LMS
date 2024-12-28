using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

/// <summary>
/// Service class for managing roles in the LMS application.
/// Provides operations to retrieve, create, update, and delete roles, with caching to improve performance.
/// </summary>
public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RoleService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleService"/> class.
    /// </summary>
    /// <param name="roleRepository">The repository for accessing role data.</param>
    /// <param name="cache">The distributed cache for caching role data.</param>
    /// <param name="logger">The logger for logging role-related operations.</param>
    public RoleService(
        IRoleRepository roleRepository,
        IDistributedCache cache,
        ILogger<RoleService> logger)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all roles from the system, utilizing caching to improve performance.
    /// Logs the process and results of fetching roles.
    /// </summary>
    /// <returns>A collection of <see cref="RoleDTO"/> objects representing all roles.</returns>
    public async Task<IEnumerable<RoleDTO>> GetAllRolesAsync()
    {
        var cacheKey = "AllRoles";
        try
        {
            _logger.LogInformation("Attempting to fetch all roles.");

            // Try to get the roles from the cache
            var cachedRoles = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedRoles))
            {
                _logger.LogInformation("All roles retrieved successfully from cache.");
                return JsonSerializer.Deserialize<IEnumerable<RoleDTO>>(cachedRoles);
            }

            _logger.LogInformation("Roles not found in cache. Fetching from database.");

            // If not found in cache, fetch from database
            var roles = await _roleRepository.GetAllAsync();

            // Map roles to DTOs
            var roleDTOs = roles.Select(role => new RoleDTO
            {
                RoleId = role.RoleId,
                RoleName = role.Name,
                Description = role.Description
            }).ToList();

            // Cache the retrieved roles
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(roleDTOs));
            _logger.LogInformation("Roles retrieved from database and stored in cache.");

            return roleDTOs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching all roles.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a specific role by its ID, utilizing caching for faster access.
    /// Logs the process and results of fetching the role.
    /// </summary>
    /// <param name="roleId">The ID of the role to retrieve.</param>
    /// <returns>A <see cref="RoleDTO"/> representing the role, or null if not found.</returns>
    public async Task<RoleDTO> GetRoleByIdAsync(int roleId)
    {
        var cacheKey = $"Role_{roleId}";
        try
        {
            _logger.LogInformation($"Attempting to fetch role with ID {roleId}.");

            // Try to get the role from the cache
            var cachedRole = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedRole))
            {
                _logger.LogInformation($"Role with ID {roleId} retrieved successfully from cache.");
                return JsonSerializer.Deserialize<RoleDTO>(cachedRole);
            }

            _logger.LogInformation($"Role with ID {roleId} not found in cache. Fetching from database.");

            // If not found in cache, fetch from database
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                _logger.LogWarning($"Role with ID {roleId} was not found in the database.");
                return null;
            }

            // Map the entity to a DTO
            var roleDTO = new RoleDTO
            {
                RoleId = role.RoleId,
                RoleName = role.Name,
                Description = role.Description
            };

            // Cache the retrieved role
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(roleDTO));
            _logger.LogInformation($"Role with ID {roleId} retrieved from database and stored in cache.");

            return roleDTO;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while fetching role with ID {roleId}.");
            throw;
        }
    }

    /// <summary>
    /// Creates a new role in the system and invalidates the roles cache to ensure freshness.
    /// Logs the process and results of creating the role.
    /// </summary>
    /// <param name="createRoleDto">The data transfer object containing the details of the role to create.</param>
    /// <returns>The ID of the newly created role.</returns>
    public async Task<int> CreateRoleAsync(CreateRoleDTO createRoleDto)
    {
        try
        {
            _logger.LogInformation("Attempting to create a new role.");

            // Map the DTO to an entity
            var role = new Role
            {
                Name = createRoleDto.RoleName,
                Description = createRoleDto.Description
            };

            // Add the new role to the database
            await _roleRepository.AddAsync(role);

            // Invalidate the cache for all roles to ensure freshness
            await _cache.RemoveAsync("AllRoles");

            _logger.LogInformation($"Role '{role.Name}' created successfully with ID {role.RoleId}.");

            return role.RoleId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new role.");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing role in the system and invalidates the related cache entries.
    /// Logs the process and results of updating the role.
    /// </summary>
    /// <param name="roleId">The ID of the role to update.</param>
    /// <param name="updateRoleDto">The data transfer object containing the updated role details.</param>
    public async Task UpdateRoleAsync(int roleId, UpdateRoleDTO updateRoleDto)
    {
        try
        {
            _logger.LogInformation($"Attempting to update role with ID {roleId}.");

            // Retrieve the role from the database
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                _logger.LogWarning($"Role with ID {roleId} was not found in the database.");
                return;
            }

            // Update the role's properties
            role.Name = updateRoleDto.RoleName;
            role.Description = updateRoleDto.Description;

            // Save the updated role to the database
            await _roleRepository.UpdateAsync(role);

            // Invalidate the cache for the specific role and all roles to ensure data freshness
            await _cache.RemoveAsync($"Role_{roleId}");
            await _cache.RemoveAsync("AllRoles");

            _logger.LogInformation($"Role with ID {roleId} updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating role with ID {roleId}.");
            throw;
        }
    }

    /// <summary>
    /// Deletes a role from the system and invalidates the related cache entries.
    /// Logs the process and results of deleting the role.
    /// </summary>
    /// <param name="roleId">The ID of the role to delete.</param>
    public async Task DeleteRoleAsync(int roleId)
    {
        try
        {
            _logger.LogInformation($"Attempting to delete role with ID {roleId}.");

            // Retrieve the role from the database
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                _logger.LogWarning($"Role with ID {roleId} was not found in the database.");
                return;
            }

            // Delete the role from the database
            await _roleRepository.DeleteAsync(roleId);

            // Invalidate the cache for the specific role and all roles to ensure data freshness
            await _cache.RemoveAsync($"Role_{roleId}");
            await _cache.RemoveAsync("AllRoles");

            _logger.LogInformation($"Role with ID {roleId} deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting role with ID {roleId}.");
            throw;
        }
    }
}
