using LMS.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDTO>> GetAllCoursesAsync();
        Task<CourseDTO> GetCourseByIdAsync(int courseId);
        Task<int> CreateCourseAsync(CreateCourseDTO createCourseDto);
        Task<bool> UpdateCourseAsync(int courseId, UpdateCourseDTO updateCourseDto);
        Task<bool> DeleteCourseAsync(int courseId);
    }
}
