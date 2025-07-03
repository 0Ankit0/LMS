using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Course
{
    public class CourseModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string? ThumbnailUrl { get; set; }

        public string InstructorId { get; set; } = string.Empty;

        public string InstructorName { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int MaxEnrollments { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public TimeSpan EstimatedDuration { get; set; }

        public string? Prerequisites { get; set; }

        public string? LearningObjectives { get; set; }

        public int EnrollmentCount { get; set; }

        public double AverageRating { get; set; }

        public List<string> Categories { get; set; } = new();

        public List<string> Tags { get; set; } = new();

        public List<ModuleModel> Modules { get; set; } = new();
    }
}
