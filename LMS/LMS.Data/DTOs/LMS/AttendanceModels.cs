using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs.LMS
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty; // Present, Absent, Late, Excused
        public string? Notes { get; set; }
        public string? SubmittedBy { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    public class BatchAttendanceResultDto
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public string UserId { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class CourseSessionDto
    {
        public int CourseId { get; set; }
        public DateTime Date { get; set; }
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
    }
}
