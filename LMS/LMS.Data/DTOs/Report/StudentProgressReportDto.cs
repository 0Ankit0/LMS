using System;

namespace LMS.Web.Repositories.DTOs
{
    public class StudentProgressReportDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public double ProgressPercentage { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public double? FinalGrade { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CompletedModules { get; set; }
        public int TotalModules { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedAssessments { get; set; }
        public int TotalAssessments { get; set; }
        public double AverageAssessmentScore { get; set; }
        public bool IsCertificateIssued { get; set; }
    }
}