using LMS.Domain.Entities;

namespace LMS.Domain.IRepositories
{
    public interface IModuleRepository
    {
        Task<IEnumerable<Module>> GetAllAsync();
        Task<Module> GetByIdAsync(int id);
        Task AddAsync(Module module);
        Task UpdateAsync(Module module);
        Task DeleteAsync(int id);
    }
}
