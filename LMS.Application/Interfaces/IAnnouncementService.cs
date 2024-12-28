using LMS.Application.DTOs;

namespace LMS.Application.Interfaces
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<AnnouncementDTO>> GetAllAnnouncementsAsync();
        Task<AnnouncementDTO> GetAnnouncementByIdAsync(int announcementId);
        Task<int> CreateAnnouncementAsync(CreateAnnouncementDTO createAnnouncementDto);
        Task<bool> UpdateAnnouncementAsync(UpdateAnnouncementDTO updateAnnouncementDto);
        Task<bool> DeleteAnnouncementAsync(int id);
    }
}