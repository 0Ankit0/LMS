using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Communication
{
    public class ForumTopicModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public int ForumId { get; set; }

        public string ForumTitle { get; set; } = string.Empty;

        public string CreatedByUserId { get; set; } = string.Empty;

        public string CreatedByUserName { get; set; } = string.Empty;

        public bool IsLocked { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastPostAt { get; set; }

        public int PostCount { get; set; }

        public List<ForumPostModel> Posts { get; set; } = new();
    }
}
