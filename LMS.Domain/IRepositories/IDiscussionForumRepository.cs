using LMS.Domain.Entities;

namespace LMS.Domain.IRepositories
{
    public interface IDiscussionForumRepository
    {
        Task<IEnumerable<DiscussionForum>> GetAllAsync();
        Task<DiscussionForum> GetByIdAsync(int id);
        Task AddAsync(DiscussionForum forum);
        Task UpdateAsync(DiscussionForum forum);
        Task DeleteAsync(int id);
    }
}
