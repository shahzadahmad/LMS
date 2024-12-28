using LMS.Application.DTOs;

namespace LMS.Application.Interfaces
{
    public interface IDiscussionForumService
    {
        Task<IEnumerable<DiscussionForumDTO>> GetAllDiscussionForumsAsync();
        Task<DiscussionForumDTO> GetDiscussionForumByIdAsync(int discussionForumId);
        Task<int> CreateDiscussionForumAsync(CreateDiscussionForumDTO createForumDTO);
        Task<bool> UpdateDiscussionForumAsync(UpdateDiscussionForumDTO updateForumDTO);
        Task<bool> DeleteDiscussionForumAsync(int discussionForumId);
    }
}
