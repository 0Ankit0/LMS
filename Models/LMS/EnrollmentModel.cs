using System.ComponentModel.DataAnnotations;

namespace LMS.Models.LMS
{
    public class EnrollmentModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseDescription { get; set; } = string.Empty;
        public string CourseThumbnail { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public double ProgressPercentage { get; set; }
        public double Progress { get; set; } // Alternative property name used in some places
        public TimeSpan TimeSpent { get; set; }
        public double? FinalGrade { get; set; }
        public bool IsCertificateIssued { get; set; }
        public DateTime? CertificateIssuedAt { get; set; }

        // Navigation properties for UI
        public CourseModel? Course { get; set; }
    }

    public class CourseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int DurationInHours { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
    }
}