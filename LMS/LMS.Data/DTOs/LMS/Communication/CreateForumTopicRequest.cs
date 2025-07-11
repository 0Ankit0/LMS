using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class CreateForumTopicRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public int ForumId { get; set; }

        [Required]
        public string InitialPost { get; set; } = string.Empty;
    }
}
