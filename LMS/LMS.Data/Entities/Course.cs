using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string? ThumbnailUrl { get; set; }

        [Required]
        public string InstructorId { get; set; } = string.Empty;

        [ForeignKey("InstructorId")]
        public virtual User Instructor { get; set; } = null!;

        public CourseLevel Level { get; set; } = CourseLevel.Beginner;

        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        public int MaxEnrollments { get; set; } = 0; // 0 means unlimited

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public TimeSpan EstimatedDuration { get; set; }

        public string? Prerequisites { get; set; }

        public string? LearningObjectives { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public virtual ICollection<Forum> Forums { get; set; } = new List<Forum>();
        public virtual ICollection<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
        public virtual ICollection<CourseTags> CourseTags { get; set; } = new List<CourseTags>();
    }

    public enum CourseLevel
    {
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3,
        Expert = 4
    }

    public enum CourseStatus
    {
        Draft = 1,
        Published = 2,
        Archived = 3,
        Suspended = 4
    }
}
