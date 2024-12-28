using LMS.Application.DTOs;

namespace LMS.Application.Interfaces
{
    public interface ILessonService
    {
        Task<IEnumerable<LessonDTO>> GetAllLessonsAsync();
        Task<LessonDTO> GetLessonByIdAsync(int lessonId);
        Task<int> CreateLessonAsync(CreateLessonDTO createLessonDto);
        Task UpdateLessonAsync(int lessonId, UpdateLessonDTO updateLessonDto);
        Task DeleteLessonAsync(int lessonId);
    }
}
