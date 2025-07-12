using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class CreateLessonRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public string? Content { get; set; }

        public int ModuleId { get; set; }

        public int Type { get; set; } = 1;

        public string? VideoUrl { get; set; }

        public string? DocumentUrl { get; set; }

        public string? ExternalUrl { get; set; }

        public TimeSpan EstimatedDuration { get; set; }

        public int OrderIndex { get; set; }

        public bool IsRequired { get; set; } = true;
    }
}
