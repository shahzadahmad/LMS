using LMS.Domain.Entities;

namespace LMS.Domain.IRepositories
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRole>> GetAllAsync();
        Task<UserRole> GetByIdAsync(int userId, int roleId);
        Task AddAsync(UserRole userRole);
        Task UpdateAsync(UserRole userRole);
        Task DeleteAsync(int userId, int roleId);
        Task<List<Role>> GetRolesByUserIdAsync(int userId);
        Task DeleteRolesByUserIdAsync(int userId);
    }
}
