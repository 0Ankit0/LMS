using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class CreateForumPostRequest
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        public int TopicId { get; set; }

        public int? ParentPostId { get; set; }
    }
}
