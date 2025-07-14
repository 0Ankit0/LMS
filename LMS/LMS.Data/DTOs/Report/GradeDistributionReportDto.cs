using System;

namespace LMS.Web.Repositories.DTOs
{
    public class GradeDistributionReportDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int AGrades { get; set; }
        public int BGrades { get; set; }
        public int CGrades { get; set; }
        public int DGrades { get; set; }
        public int FGrades { get; set; }
        public double AverageGrade { get; set; }
        public double MedianGrade { get; set; }
        public double HighestGrade { get; set; }
        public double LowestGrade { get; set; }
        public double StandardDeviation { get; set; }
        public string GradeRange { get; set; } = string.Empty;
        public DateTime ReportGeneratedDate { get; set; }
    }
}