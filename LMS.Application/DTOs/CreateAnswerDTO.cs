using System.ComponentModel.DataAnnotations;

namespace LMS.Application.DTOs
{
    public class CreateAnswerDTO
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string Content { get; set; }

        public bool IsCorrect { get; set; }
    }
}
