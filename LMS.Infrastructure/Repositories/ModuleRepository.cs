using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly LMSDbContext _context;

        public ModuleRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Module>> GetAllAsync()
        {
            return await _context.Modules.Include(m => m.Lessons).ToListAsync();
        }

        public async Task<Module> GetByIdAsync(int id)
        {
            return await _context.Modules
                .Include(m => m.Lessons)
                .FirstOrDefaultAsync(m => m.ModuleId == id);
        }

        public async Task AddAsync(Module module)
        {
            await _context.Modules.AddAsync(module);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Module module)
        {
            _context.Modules.Update(module);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var module = await _context.Modules.FindAsync(id);
            if (module != null)
            {
                _context.Modules.Remove(module);
                await _context.SaveChangesAsync();
            }
        }
    }
}
