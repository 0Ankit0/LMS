using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities
{
    // Progress Tracking Models
    public class ModuleProgress
    {
        [Key]
        public int Id { get; set; }

        public int EnrollmentId { get; set; }

        [ForeignKey("EnrollmentId")]
        public virtual Enrollment Enrollment { get; set; } = null!;

        public int ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public virtual Module Module { get; set; } = null!;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public double ProgressPercentage { get; set; } = 0;

        public TimeSpan TimeSpent { get; set; } = TimeSpan.Zero;

        public bool IsCompleted => CompletedAt.HasValue;

        // Navigation Properties
        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }

    public class LessonProgress
    {
        [Key]
        public int Id { get; set; }

        public int ModuleProgressId { get; set; }

        [ForeignKey("ModuleProgressId")]
        public virtual ModuleProgress ModuleProgress { get; set; } = null!;

        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } = null!;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public TimeSpan TimeSpent { get; set; } = TimeSpan.Zero;

        public double ProgressPercentage { get; set; } = 0;

        public bool IsCompleted => CompletedAt.HasValue;

        // SCORM/xAPI tracking
        public string? ScormData { get; set; }
        public string? XApiStatements { get; set; }
    }

    public class AssessmentAttempt
    {
        [Key]
        public int Id { get; set; }

        public int EnrollmentId { get; set; }

        [ForeignKey("EnrollmentId")]
        public virtual Enrollment Enrollment { get; set; } = null!;

        public int AssessmentId { get; set; }

        [ForeignKey("AssessmentId")]
        public virtual Assessment Assessment { get; set; } = null!;

        public int AttemptNumber { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public double? Score { get; set; }

        public double? Percentage { get; set; }

        public bool IsPassed { get; set; }

        public TimeSpan? TimeTaken { get; set; }

        public AssessmentAttemptStatus Status { get; set; } = AssessmentAttemptStatus.InProgress;

        // Navigation Properties
        public virtual ICollection<QuestionResponse> Responses { get; set; } = new List<QuestionResponse>();
    }

    public class QuestionResponse
    {
        [Key]
        public int Id { get; set; }

        public int AttemptId { get; set; }

        [ForeignKey("AttemptId")]
        public virtual AssessmentAttempt Attempt { get; set; } = null!;

        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;

        public int? SelectedOptionId { get; set; }

        [ForeignKey("SelectedOptionId")]
        public virtual QuestionOption? SelectedOption { get; set; }

        public string? TextResponse { get; set; }

        public bool IsCorrect { get; set; }

        public double PointsEarned { get; set; }

        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }

    public enum AssessmentAttemptStatus
    {
        InProgress = 1,
        Completed = 2,
        Abandoned = 3,
        TimedOut = 4,
        PendingManualGrading = 5
    }
}
