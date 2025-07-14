using System;

namespace LMS.Web.Repositories.DTOs
{
    public class AssessmentPerformanceReportDto
    {
        public int AssessmentId { get; set; }
        public string AssessmentTitle { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public int PassedAttempts { get; set; }
        public int FailedAttempts { get; set; }
        public double PassRate { get; set; }
        public double AverageScore { get; set; }
        public double HighestScore { get; set; }
        public double LowestScore { get; set; }
        public double AverageCompletionTime { get; set; } // in minutes
        public double PassingScore { get; set; }
        public int UniqueStudents { get; set; }
        public DateTime? LastAttemptDate { get; set; }
    }
}