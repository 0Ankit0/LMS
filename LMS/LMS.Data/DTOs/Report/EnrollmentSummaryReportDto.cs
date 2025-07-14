using System;

namespace LMS.Web.Repositories.DTOs
{
    public class EnrollmentSummaryReportDto
    {
        public DateTime ReportDate { get; set; }
        public int TotalEnrollments { get; set; }
        public int NewEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public int ActiveEnrollments { get; set; }
        public int DroppedEnrollments { get; set; }
        public int SuspendedEnrollments { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string Period { get; set; } = string.Empty; // Daily, Weekly, Monthly
        public double AverageProgressPercentage { get; set; }
        public int CertificatesIssued { get; set; }
    }
}