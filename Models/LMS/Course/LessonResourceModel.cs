using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Course
{
    public class LessonResourceModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Type { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string? ExternalUrl { get; set; }

        public long FileSize { get; set; }

        public string ContentType { get; set; } = string.Empty;

        public bool IsDownloadable { get; set; } = true;
    }
}
