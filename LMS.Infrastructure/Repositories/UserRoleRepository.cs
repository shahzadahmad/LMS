using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly LMSDbContext _context;

        public UserRoleRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserRole>> GetAllAsync()
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<UserRole> GetByIdAsync(int userId, int roleId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task AddAsync(UserRole userRole)
        {
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserRole userRole)
        {
            _context.UserRoles.Update(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId, int roleId)
        {
            var userRole = await GetByIdAsync(userId, roleId);
            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Role>> GetRolesByUserIdAsync(int userId)
        {
            // Query to get the role IDs for the specified user
            var roleIds = await _context.UserRoles
                                        .Where(ur => ur.UserId == userId)
                                        .Select(ur => ur.RoleId)
                                        .ToListAsync();

            // Query to get the roles associated with these role IDs
            var roles = await _context.Roles
                                      .Where(r => roleIds.Contains(r.RoleId))
                                      .ToListAsync();

            return roles;
        }

        public async Task DeleteRolesByUserIdAsync(int userId)
        {
            // Retrieve all UserRole entries associated with the userId
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            // Remove the entries from the database context
            _context.UserRoles.RemoveRange(userRoles);

            // Save changes to the database
            await _context.SaveChangesAsync();
        }
    }
}