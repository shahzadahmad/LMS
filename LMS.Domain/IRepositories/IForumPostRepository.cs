using LMS.Domain.Entities;

namespace LMS.Domain.IRepositories
{
    public interface IForumPostRepository
    {
        Task AddAsync(ForumPost forumPost);
        Task UpdateAsync(ForumPost forumPost);
        Task<ForumPost> GetByIdAsync(int postId);
        Task<IEnumerable<ForumPost>> GetAllAsync();
        Task DeleteAsync(int postId);
    }
}
