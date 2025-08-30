using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities
{
    /// <summary>
    /// Student-specific properties extending User
    /// </summary>
    public class Student
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [StringLength(50)]
        public string? StudentIdNumber { get; set; }

        public DateTime? EnrollmentDate { get; set; }

        public DateTime? GraduationDate { get; set; }

        [StringLength(200)]
        public string? EmergencyContactName { get; set; }

        [StringLength(50)]
        public string? EmergencyContactPhone { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalPoints { get; set; } = 0;

        [Range(1, int.MaxValue)]
        public int Level { get; set; } = 1;

        // Navigation properties
        public virtual ICollection<ParentStudentLink> ParentLinks { get; set; } = new List<ParentStudentLink>();
    }

    /// <summary>
    /// Instructor-specific properties extending User
    /// </summary>
    public class Instructor
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(500)]
        public string? OfficeHours { get; set; }

        public DateTime? HireDate { get; set; }
    }

    /// <summary>
    /// Parent/Guardian-specific properties extending User
    /// </summary>
    public class Parent
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;

        [StringLength(50)]
        public string? PreferredContactMethod { get; set; }

        // Navigation properties
        public virtual ICollection<ParentStudentLink> StudentLinks { get; set; } = new List<ParentStudentLink>();
    }

    /// <summary>
    /// Junction table for many-to-many relationship between parents and students
    /// </summary>
    public class ParentStudentLink
    {
        [Required]
        public string ParentId { get; set; } = string.Empty;

        [ForeignKey("ParentId")]
        public virtual Parent Parent { get; set; } = null!;

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// User-specific preferences and settings
    /// </summary>
    public class UserSettings
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Theme { get; set; } = "Light";

        [Required]
        [StringLength(10)]
        public string Language { get; set; } = "en-US";

        public bool EmailNotifications { get; set; } = true;

        public bool SmsNotifications { get; set; } = false;

        public bool PushNotifications { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Logs significant user actions within the system
    /// </summary>
    public class UserActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public ActivityType Type { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }
    }

    public enum ActivityType
    {
        Login,
        Logout,
        CourseEnrollment,
        CourseCompletion,
        AssessmentAttempt,
        AssessmentCompletion,
        LessonView,
        ModuleCompletion,
        AchievementEarned,
        ProfileUpdate,
        FileUpload,
        ForumPost,
        MessageSent
    }
}
