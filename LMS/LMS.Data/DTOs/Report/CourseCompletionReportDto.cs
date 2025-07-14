using System;

namespace LMS.Web.Repositories.DTOs
{
    public class CourseCompletionReportDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int TotalEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public int ActiveEnrollments { get; set; }
        public int DroppedEnrollments { get; set; }
        public double CompletionRate { get; set; }
        public double AverageCompletionTime { get; set; } // in days
        public double AverageFinalGrade { get; set; }
        public int CertificatesIssued { get; set; }
        public DateTime CourseStartDate { get; set; }
        public DateTime? CourseEndDate { get; set; }
        public string CourseStatus { get; set; } = string.Empty;
    }
}