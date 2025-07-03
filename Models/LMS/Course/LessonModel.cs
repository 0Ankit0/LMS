using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Course
{
    public class LessonModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public string? Content { get; set; }

        public int ModuleId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string? VideoUrl { get; set; }

        public string? DocumentUrl { get; set; }

        public string? ExternalUrl { get; set; }

        public TimeSpan EstimatedDuration { get; set; }

        public int OrderIndex { get; set; }

        public bool IsRequired { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public bool IsCompleted { get; set; }

        public double ProgressPercentage { get; set; }

        public List<LessonResourceModel> Resources { get; set; } = new();
    }
}
