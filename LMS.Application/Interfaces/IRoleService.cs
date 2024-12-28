using LMS.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDTO>> GetAllRolesAsync();
        Task<RoleDTO> GetRoleByIdAsync(int roleId);
        Task<int> CreateRoleAsync(CreateRoleDTO createRoleDto);
        Task UpdateRoleAsync(int roleId, UpdateRoleDTO updateRoleDto);
        Task DeleteRoleAsync(int roleId);
    }
}
