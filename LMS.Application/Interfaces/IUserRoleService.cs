using LMS.Application.DTOs;

namespace LMS.Application.Interfaces
{
    public interface IUserRoleService
    {
        Task<IEnumerable<UserRoleDTO>> GetAllUserRolesAsync();
        Task<UserRoleDTO> GetUserRoleAsync(int userId, int roleId);
        Task AssignUserRoleAsync(CreateUserRoleDTO createUserRoleDto);
        Task RemoveUserRoleAsync(int userId, int roleId);
    }
}
