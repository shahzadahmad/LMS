using LMS.Domain.Entities;
using LMS.Domain.IRepositories;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories
{
    public class DiscussionForumRepository : IDiscussionForumRepository
    {
        private readonly LMSDbContext _context;

        public DiscussionForumRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DiscussionForum>> GetAllAsync()
        {
            return await _context.DiscussionForums
                                 .Include(df => df.CreatedByUser)
                                 .Include(df => df.Course)
                                 .ToListAsync();
        }

        public async Task<DiscussionForum> GetByIdAsync(int id)
        {
            return await _context.DiscussionForums
                                 .Include(df => df.CreatedByUser)
                                 .Include(df => df.Course)
                                 .FirstOrDefaultAsync(df => df.ForumId == id);
        }

        public async Task AddAsync(DiscussionForum forum)
        {
            _context.DiscussionForums.Add(forum);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DiscussionForum forum)
        {
            _context.DiscussionForums.Update(forum);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var forum = await GetByIdAsync(id);
            if (forum != null)
            {
                _context.DiscussionForums.Remove(forum);
                await _context.SaveChangesAsync();
            }
        }
    }
}
