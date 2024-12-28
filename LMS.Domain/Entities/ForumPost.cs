using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Domain.Entities
{
    public class ForumPost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }

        [ForeignKey("Forum")]
        public int ForumId { get; set; }
        public DiscussionForum Forum { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("CreatedByUser")]
        public int CreatedBy { get; set; }
        public User CreatedByUser { get; set; }

        [ForeignKey("ParentPost")]
        public int? ParentPostId { get; set; }
        public ForumPost ParentPost { get; set; }

        public ICollection<ForumPost> Replies { get; set; } = new List<ForumPost>();
    }
}
