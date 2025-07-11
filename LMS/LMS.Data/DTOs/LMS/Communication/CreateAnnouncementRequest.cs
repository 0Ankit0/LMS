using System.ComponentModel.DataAnnotations;
using LMS.Data.Entities;

namespace LMS.Data.DTOs
{
    public class CreateAnnouncementRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? CourseId { get; set; }

        [Required]
        public AnnouncementType Type { get; set; } = AnnouncementType.General;

        public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;

        public DateTime? ExpiresAt { get; set; }

        public bool SendEmail { get; set; } = false;

        public bool SendSms { get; set; } = false;
    }
}
