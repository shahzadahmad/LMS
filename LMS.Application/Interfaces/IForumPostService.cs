using LMS.Application.DTOs;
using LMS.Domain.Entities;

namespace LMS.Application.Interfaces
{
    public interface IForumPostService
    {
        Task<ForumPostDTO> GetForumPostByIdAsync(int postId);
        Task<IEnumerable<ForumPostDTO>> GetAllForumPostsAsync();
        Task<int> CreateForumPostAsync(CreateForumPostDTO createForumPostDto);
        Task<bool> UpdateForumPostAsync(int postId, UpdateForumPostDTO updateForumPostDto);
        Task<bool> DeleteForumPostAsync(int postId);
    }
}
