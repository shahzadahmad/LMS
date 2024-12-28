using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LMSDbContext _context;

        public UserRepository(LMSDbContext context)
        {
            _context = context;
        }

        // Retrieves all users including their associated roles
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles) // Include the user's roles
                .ThenInclude(ur => ur.Role) // Include the role details
                .ToListAsync();
        }

        // Retrieves a user by their ID, including their associated roles
        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.UserRoles) // Include the user's roles
                .ThenInclude(ur => ur.Role) // Include the role details
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        // Retrieves a user by their username, including their associated roles
        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.UserRoles) // Include the user's roles
                .ThenInclude(ur => ur.Role) // Include the role details
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        // Retrieves a user by their email, including their associated roles
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles) // Include the user's roles
                .ThenInclude(ur => ur.Role) // Include the role details
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // Adds a new user to the database
        public async Task<User> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync(); // Save changes to the database
            return user; // Return the created user
        }

        // Updates an existing user in the database
        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user); // Mark the user entity as modified
            await _context.SaveChangesAsync(); // Save changes to the database
        }

        // Deletes a user by their ID from the database
        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id); // Find the user by ID
            if (user != null)
            {
                _context.Users.Remove(user); // Remove the user entity
                await _context.SaveChangesAsync(); // Save changes to the database
            }
        }
    }
}
