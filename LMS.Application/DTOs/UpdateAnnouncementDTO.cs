using System.ComponentModel.DataAnnotations;

namespace LMS.Application.DTOs
{
    public class UpdateAnnouncementDTO
    {
        [Required]
        public int AnnouncementId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int CreatedBy { get; set; } // The user ID of the creator

        public int? CourseId { get; set; } // Optional, for associating with a course
    }
}
