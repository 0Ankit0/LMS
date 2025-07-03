using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data
{
    public class Assessment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int? CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        public int? ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public virtual Module? Module { get; set; }

        public int? LessonId { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }

        public AssessmentType Type { get; set; } = AssessmentType.Quiz;

        public int MaxAttempts { get; set; } = 1;

        public TimeSpan TimeLimit { get; set; } = TimeSpan.Zero; // Zero means no time limit

        public double PassingScore { get; set; } = 70.0;

        public bool IsRandomized { get; set; } = false;

        public bool ShowCorrectAnswers { get; set; } = true;

        public bool ShowScoreImmediately { get; set; } = true;

        public DateTime? AvailableFrom { get; set; }

        public DateTime? AvailableUntil { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<AssessmentAttempt> Attempts { get; set; } = new List<AssessmentAttempt>();
    }

    public enum AssessmentType
    {
        Quiz = 1,
        Exam = 2,
        Assignment = 3,
        Survey = 4,
        SelfAssessment = 5
    }
}
