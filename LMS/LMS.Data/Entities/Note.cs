using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities
{
    public class Note
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int? CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        public int? LessonId { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }

        public bool IsPrivate { get; set; } = true;

        public bool IsPinned { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // For soft delete
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        // Tags and categories
        [StringLength(500)]
        public string? Tags { get; set; }

        public NoteType Type { get; set; } = NoteType.General;

        public NotePriority Priority { get; set; } = NotePriority.Normal;
    }

    public enum NoteType
    {
        General = 1,
        StudyNote = 2,
        Reminder = 3,
        Question = 4,
        Summary = 5,
        Highlight = 6
    }

    public enum NotePriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }
}
