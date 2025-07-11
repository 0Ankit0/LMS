using LMS.Data.DTOs;
using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class AssessmentModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int? CourseId { get; set; }

        public string? CourseName { get; set; }

        public int? ModuleId { get; set; }

        public string? ModuleName { get; set; }

        public int? LessonId { get; set; }

        public string? LessonName { get; set; }

        public string Type { get; set; } = string.Empty;

        public int MaxAttempts { get; set; } = 1;

        public TimeSpan TimeLimit { get; set; } = TimeSpan.Zero;

        public double PassingScore { get; set; } = 70.0;

        public bool IsRandomized { get; set; } = false;

        public bool ShowCorrectAnswers { get; set; } = true;

        public bool ShowScoreImmediately { get; set; } = true;

        public DateTime? AvailableFrom { get; set; }

        public DateTime? AvailableUntil { get; set; }

        public bool IsActive { get; set; } = true;

        public bool AllowRetakes { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public List<QuestionModel> Questions { get; set; } = new();

        public AssessmentAttemptModel? LastAttempt { get; set; }

        public int AttemptsUsed { get; set; }

        public bool CanAttempt { get; set; }

        // Navigation properties for UI display
        public CourseModel? Course { get; set; }
        public ModuleModel? Module { get; set; }
    }

    //public class AssessmentCourseModel
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; } = string.Empty;
    //    public string Title { get; set; } = string.Empty;
    //}

    public class ModuleModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
