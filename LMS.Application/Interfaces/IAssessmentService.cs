using LMS.Application.DTOs;

namespace LMS.Application.Interfaces
{
    public interface IAssessmentService
    {
        Task<IEnumerable<AssessmentDTO>> GetAllAssessmentsAsync();
        Task<AssessmentDTO> GetAssessmentByIdAsync(int assessmentId);
        Task<int> CreateAssessmentAsync(CreateAssessmentDTO createAssessmentDto);
        Task<bool> UpdateAssessmentAsync(int assessmentId, UpdateAssessmentDTO updateAssessmentDto);
        Task<bool> DeleteAssessmentAsync(int assessmentId);
    }
}
