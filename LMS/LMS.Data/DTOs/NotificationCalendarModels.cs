using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Info, Warning, Success, Error
        public string? Category { get; set; } // Course, Assessment, System, etc.
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class CalendarEventModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAllDay { get; set; }
        public string? Location { get; set; }
        public string Type { get; set; } = string.Empty; // Course, Assignment, Exam, Meeting, etc.
        public string? Color { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public List<string> Attendees { get; set; } = new();
    }

    public record CreateCalendarEventRequest(
        [Required][StringLength(200)] string Title,
        [StringLength(1000)] string? Description,
        [Required] DateTime StartDate,
        [Required] DateTime EndDate,
        bool IsAllDay,
        [StringLength(200)] string? Location,
        [Required] string Type,
        string? Color,
        int? CourseId,
        bool IsRecurring = false,
        string? RecurrencePattern = null,
        List<string>? AttendeeIds = null
    );

    public record UpdateCalendarEventRequest(
        [StringLength(200)] string? Title,
        [StringLength(1000)] string? Description,
        DateTime? StartDate,
        DateTime? EndDate,
        bool? IsAllDay,
        [StringLength(200)] string? Location,
        string? Type,
        string? Color,
        int? CourseId,
        bool? IsRecurring,
        string? RecurrencePattern,
        List<string>? AttendeeIds
    );
}
