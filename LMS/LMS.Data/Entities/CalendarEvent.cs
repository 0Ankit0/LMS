using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities
{
    public class CalendarEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsAllDay { get; set; } = false;

        [StringLength(200)]
        public string? Location { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Course, Assignment, Exam, Meeting, etc.

        [StringLength(20)]
        public string? Color { get; set; }

        public int? CourseId { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRecurring { get; set; } = false;

        [StringLength(500)]
        public string? RecurrencePattern { get; set; }

        [StringLength(2000)]
        public string? Attendees { get; set; } // JSON string for attendee list

        // Navigation properties
        public Course? Course { get; set; }
        public User? Creator { get; set; }
    }
}
