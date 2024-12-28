using System.ComponentModel.DataAnnotations;

namespace LMS.Application.DTOs
{
    public class CreateQuestionDTO
    {
        [Required]
        public int AssessmentId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Content length can't exceed 1000 characters.")]
        public string Content { get; set; }
    }
}
