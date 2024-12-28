using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.DTOs
{
    public class CreateForumPostDTO
    {
        public int ForumId { get; set; }
        public string Content { get; set; }
        public int CreatedBy { get; set; }
        public int? ParentPostId { get; set; }
    }
}
