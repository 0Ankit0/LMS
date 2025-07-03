using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Assessment
{
    public class CreateAssessmentModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int? CourseId { get; set; }

        public int? ModuleId { get; set; }

        public int? LessonId { get; set; }

        public int Type { get; set; } = 1;

        public int MaxAttempts { get; set; } = 1;

        public TimeSpan TimeLimit { get; set; } = TimeSpan.Zero;

        public double PassingScore { get; set; } = 70.0;

        public bool IsRandomized { get; set; } = false;

        public bool ShowCorrectAnswers { get; set; } = true;

        public bool ShowScoreImmediately { get; set; } = true;

        public DateTime? AvailableFrom { get; set; }

        public DateTime? AvailableUntil { get; set; }
    }
}
