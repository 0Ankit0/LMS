using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Communication
{
    public class AnnouncementModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string AuthorId { get; set; } = string.Empty;

        public string AuthorName { get; set; } = string.Empty;

        public int? CourseId { get; set; }

        public string? CourseName { get; set; }

        public string Priority { get; set; } = string.Empty;

        public DateTime PublishedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; }

        public bool SendEmail { get; set; }

        public bool SendSms { get; set; }
    }
}
