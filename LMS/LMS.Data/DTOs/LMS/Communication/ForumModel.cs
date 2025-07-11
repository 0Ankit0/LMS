using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class ForumModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int? CourseId { get; set; }

        public string? CourseName { get; set; }

        public bool IsGeneral { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public int TopicCount { get; set; }

        public DateTime? LastPostAt { get; set; }

        public List<ForumTopicModel> Topics { get; set; } = new();
    }
}
