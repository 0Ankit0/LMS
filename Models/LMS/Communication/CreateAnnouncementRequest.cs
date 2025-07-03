using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Communication
{
    public class CreateAnnouncementRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? CourseId { get; set; }

        public int Priority { get; set; } = 2; // Normal priority

        public DateTime? ExpiresAt { get; set; }

        public bool SendEmail { get; set; } = false;

        public bool SendSms { get; set; } = false;
    }
}
