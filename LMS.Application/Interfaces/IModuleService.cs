using LMS.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces
{
    public interface IModuleService
    {
        Task<IEnumerable<ModuleDTO>> GetAllModulesAsync();
        Task<ModuleDTO> GetModuleByIdAsync(int moduleId);
        Task<int> CreateModuleAsync(CreateModuleDTO createModuleDto);
        Task UpdateModuleAsync(int moduleId, UpdateModuleDTO updateModuleDto);
        Task DeleteModuleAsync(int moduleId);
    }
}
