using LMS.Application.DTOs;
using LMS.Domain.Entities;

namespace LMS.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> GetUserByIdAsync(int userId);
        Task<UserDTO> GetUserByEmailAsync(string email);
        Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDto);
        Task<UserDTO> UpdateUserAsync(int userId, UpdateUserDTO updateUserDto);
        Task<bool> DeleteUserAsync(int userId);
        Task AssignRolesAsync(int userId, List<int> roleIds);
        Task<UserDTO> AuthenticateUserAsync(string username, string password);
        string GenerateJwtToken(UserDTO user);
    }
}
