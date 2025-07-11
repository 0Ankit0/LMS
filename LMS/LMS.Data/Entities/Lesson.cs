using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public string? Content { get; set; } // HTML content

        public int ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public virtual Module Module { get; set; } = null!;

        public LessonType Type { get; set; } = LessonType.Text;

        public string? VideoUrl { get; set; }

        public string? DocumentUrl { get; set; }

        public string? ExternalUrl { get; set; }

        public TimeSpan EstimatedDuration { get; set; }

        public int OrderIndex { get; set; }

        public bool IsRequired { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // SCORM/xAPI Support
        public bool IsScormContent { get; set; }
        public string? ScormLaunchUrl { get; set; }
        public string? XApiActivityId { get; set; }

        // Navigation Properties
        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
        public virtual ICollection<LessonResource> Resources { get; set; } = new List<LessonResource>();
    }
    public class LessonResource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } = null!;

        public ResourceType Type { get; set; } = ResourceType.Document;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public string? ExternalUrl { get; set; }

        public long FileSize { get; set; }

        public string ContentType { get; set; } = string.Empty;

        public bool IsDownloadable { get; set; } = true;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    public enum LessonType
    {
        Text = 1,
        Video = 2,
        Document = 3,
        Interactive = 4,
        Quiz = 5,
        SCORM = 6,
        External = 7,
        LiveSession = 8
    }
}
