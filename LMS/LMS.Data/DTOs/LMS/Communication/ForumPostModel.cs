using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class ForumPostModel
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public int TopicId { get; set; }

        public string TopicTitle { get; set; } = string.Empty;

        public string AuthorId { get; set; } = string.Empty;

        public string AuthorName { get; set; } = string.Empty;

        public string? AuthorProfilePictureUrl { get; set; }

        public int? ParentPostId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public List<ForumPostModel> Replies { get; set; } = new();
    }
}
