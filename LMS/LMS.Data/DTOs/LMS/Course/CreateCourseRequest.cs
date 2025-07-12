using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class CreateCourseRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string? ThumbnailUrl { get; set; }

        [Required]
        public string InstructorId { get; set; } = string.Empty;

        public int Level { get; set; } = 1;

        public int Status { get; set; } = 1; // 1 = Draft, 2 = Published, etc.

        public int MaxEnrollments { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow.AddDays(7);

        public DateTime? EndDate { get; set; }

        public TimeSpan EstimatedDuration { get; set; }

        public string? Prerequisites { get; set; }

        public string? LearningObjectives { get; set; }

        public bool IsActive { get; set; } = true;

        public List<int> CategoryIds { get; set; } = new();

        public List<int> TagIds { get; set; } = new();
    }
}
