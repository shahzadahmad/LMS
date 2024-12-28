using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories
{
    public class ForumPostRepository : IForumPostRepository
    {
        private readonly LMSDbContext _context;

        public ForumPostRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ForumPost forumPost)
        {
            await _context.ForumPosts.AddAsync(forumPost);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ForumPost forumPost)
        {
            _context.ForumPosts.Update(forumPost);
            await _context.SaveChangesAsync();
        }

        public async Task<ForumPost> GetByIdAsync(int postId)
        {
            return await _context.ForumPosts
                .Include(fp => fp.CreatedByUser)
                .Include(fp => fp.Forum)
                .Include(fp => fp.ParentPost)
                .FirstOrDefaultAsync(fp => fp.PostId == postId);
        }

        public async Task<IEnumerable<ForumPost>> GetAllAsync()
        {
            return await _context.ForumPosts
                .Include(fp => fp.CreatedByUser)
                .Include(fp => fp.Forum)
                .Include(fp => fp.ParentPost)
                .ToListAsync();
        }

        public async Task DeleteAsync(int postId)
        {
            var forumPost = await GetByIdAsync(postId);
            if (forumPost != null)
            {
                _context.ForumPosts.Remove(forumPost);
                await _context.SaveChangesAsync();
            }
        }
    }

}
