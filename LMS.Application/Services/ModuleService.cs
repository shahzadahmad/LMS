using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMS.Application.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ILogger<ModuleService> _logger;
        private readonly IDistributedCache _cache;

        public ModuleService(IModuleRepository moduleRepository, ILogger<ModuleService> logger, IDistributedCache cache)
        {
            _moduleRepository = moduleRepository ?? throw new ArgumentNullException(nameof(moduleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Retrieves all modules, either from cache or by querying the database.
        /// Logs cache hits and database queries, and handles errors.
        /// </summary>
        /// <returns>A list of <see cref="ModuleDTO"/>s.</returns>
        public async Task<IEnumerable<ModuleDTO>> GetAllModulesAsync()
        {
            try
            {
                const string cacheKey = "GetAllModules";

                // Check if the data is in cache
                var cachedModules = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedModules))
                {
                    _logger.LogInformation("Modules retrieved from cache.");
                    return JsonSerializer.Deserialize<IEnumerable<ModuleDTO>>(cachedModules);
                }

                // If not in cache, fetch from database
                _logger.LogInformation("Modules not found in cache. Querying database.");
                var modules = await _moduleRepository.GetAllAsync();
                if (modules == null || !modules.Any())
                {
                    _logger.LogInformation("No modules found in the database.");
                    return Enumerable.Empty<ModuleDTO>();
                }

                // Convert entities to DTOs
                var moduleDTOs = modules.Select(module => new ModuleDTO
                {
                    ModuleId = module.ModuleId,
                    ModuleName = module.Title,
                    Description = module.Content,
                    Lessons = module.Lessons.Select(lesson => new LessonDTO
                    {
                        LessonId = lesson.LessonId,
                        LessonName = lesson.Title,
                        Content = lesson.Content
                    }).ToList()
                }).ToList();

                // Cache the result
                var serializedModules = JsonSerializer.Serialize(moduleDTOs);
                await _cache.SetStringAsync(cacheKey, serializedModules);

                _logger.LogInformation("Modules retrieved from database and cached.");
                return moduleDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all modules.");
                throw; // Re-throw to handle at higher level if needed
            }
        }

        /// <summary>
        /// Retrieves a specific module by its ID, either from cache or by querying the database.
        /// Logs cache hits and database queries, and handles errors.
        /// </summary>
        /// <param name="moduleId">The ID of the module.</param>
        /// <returns>A <see cref="ModuleDTO"/> or null if not found.</returns>
        public async Task<ModuleDTO> GetModuleByIdAsync(int moduleId)
        {
            try
            {
                var cacheKey = $"GetModule_{moduleId}";

                // Check if the data is in cache
                var cachedModule = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedModule))
                {
                    _logger.LogInformation("Module {ModuleId} retrieved from cache.", moduleId);
                    return JsonSerializer.Deserialize<ModuleDTO>(cachedModule);
                }

                // If not in cache, fetch from database
                _logger.LogInformation("Module {ModuleId} not found in cache. Querying database.", moduleId);
                var module = await _moduleRepository.GetByIdAsync(moduleId);
                if (module == null)
                {
                    _logger.LogWarning("Module {ModuleId} not found in the database.", moduleId);
                    return null;
                }

                // Convert entity to DTO
                var moduleDTO = new ModuleDTO
                {
                    ModuleId = module.ModuleId,
                    ModuleName = module.Title,
                    Description = module.Content,
                    Lessons = module.Lessons.Select(lesson => new LessonDTO
                    {
                        LessonId = lesson.LessonId,
                        LessonName = lesson.Title,
                        Content = lesson.Content
                    }).ToList()
                };

                // Cache the result
                var serializedModule = JsonSerializer.Serialize(moduleDTO);
                await _cache.SetStringAsync(cacheKey, serializedModule);

                _logger.LogInformation("Module {ModuleId} retrieved from database and cached.", moduleId);
                return moduleDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving module {ModuleId}.", moduleId);
                throw; // Re-throw to handle at higher level if needed
            }
        }

        /// <summary>
        /// Creates a new module in the database and invalidates the cache.
        /// Logs the creation process and handles errors.
        /// </summary>
        /// <param name="createModuleDto">The <see cref="CreateModuleDTO"/> containing module creation details.</param>
        /// <returns>The ID of the newly created module.</returns>
        public async Task<int> CreateModuleAsync(CreateModuleDTO createModuleDto)
        {
            try
            {
                // Map DTO to entity
                var module = new Module
                {
                    Title = createModuleDto.ModuleName,
                    Content = createModuleDto.Description,
                    CourseId = createModuleDto.CourseId
                };

                // Add module to the repository
                await _moduleRepository.AddAsync(module);

                _logger.LogInformation("Module {ModuleId} created successfully.", module.ModuleId);

                // Invalidate the cache for the module list
                await _cache.RemoveAsync("GetAllModules");

                return module.ModuleId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a module.");
                throw; // Re-throw to handle at higher level if needed
            }
        }

        /// <summary>
        /// Updates an existing module in the database and invalidates the cache.
        /// Logs the update process and handles errors.
        /// </summary>
        /// <param name="moduleId">The ID of the module to update.</param>
        /// <param name="updateModuleDto">The <see cref="UpdateModuleDTO"/> containing update details.</param>
        public async Task UpdateModuleAsync(int moduleId, UpdateModuleDTO updateModuleDto)
        {
            try
            {
                // Fetch the module from the repository
                var module = await _moduleRepository.GetByIdAsync(moduleId);
                if (module == null)
                {
                    _logger.LogWarning("Module {ModuleId} not found for update.", moduleId);
                    return;
                }

                // Update module details
                module.Title = updateModuleDto.ModuleName;
                module.Content = updateModuleDto.Description;
                module.UpdatedAt = DateTime.UtcNow;

                // Update module in the repository
                await _moduleRepository.UpdateAsync(module);

                _logger.LogInformation("Module {ModuleId} updated successfully.", moduleId);

                // Invalidate the cache for this module and the module list
                await _cache.RemoveAsync($"GetModule_{moduleId}");
                await _cache.RemoveAsync("GetAllModules");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating module {ModuleId}.", moduleId);
                throw; // Re-throw to handle at higher level if needed
            }
        }

        /// <summary>
        /// Deletes a module by its ID and invalidates the cache.
        /// Logs the deletion process and handles errors.
        /// </summary>
        /// <param name="moduleId">The ID of the module to delete.</param>
        public async Task DeleteModuleAsync(int moduleId)
        {
            try
            {
                // Delete the module from the repository
                await _moduleRepository.DeleteAsync(moduleId);

                _logger.LogInformation("Module {ModuleId} deleted successfully.", moduleId);

                // Invalidate the cache for this module and the module list
                await _cache.RemoveAsync($"GetModule_{moduleId}");
                await _cache.RemoveAsync("GetAllModules");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting module {ModuleId}.", moduleId);
                throw; // Re-throw to handle at higher level if needed
            }
        }
    }
}
