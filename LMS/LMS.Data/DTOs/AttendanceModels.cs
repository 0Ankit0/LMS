using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class AttendanceModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime SessionDate { get; set; }
        public string Status { get; set; } = string.Empty; // Present, Absent, Late, Excused
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public record BatchAttendanceDto(
        int CourseId,
        DateTime SessionDate,
        List<AttendanceRecordDto> AttendanceRecords,
        string? SessionNotes = null
    );

    public record AttendanceRecordDto(
        string UserId,
        string Status,
        string? Notes = null
    );

    public record UpdateAttendanceDto(
        string Status,
        string? Notes = null
    );

    public class AttendanceSummaryModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public decimal AttendancePercentage { get; set; }
    }

    public class CourseSessionModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public DateTime SessionDate { get; set; }
        public string? Topic { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public int AttendanceCount { get; set; }
        public int TotalStudents { get; set; }
    }
}
