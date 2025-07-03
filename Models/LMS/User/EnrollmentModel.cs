using System.ComponentModel.DataAnnotations;

namespace LMS.Models.User
{
    public class EnrollmentModel
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        public string CourseThumbnailUrl { get; set; } = string.Empty;

        public string CourseDescription { get; set; } = string.Empty;

        public string CourseThumbnail { get; set; } = string.Empty; // Alternative property name

        public DateTime EnrolledAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? LastAccessedAt { get; set; }

        public string Status { get; set; } = string.Empty;

        public double ProgressPercentage { get; set; }

        public double Progress { get; set; } // Alternative property name for ProgressPercentage

        public TimeSpan TimeSpent { get; set; }

        public double? FinalGrade { get; set; }

        public bool IsCertificateIssued { get; set; }

        public DateTime? CertificateIssuedAt { get; set; }

        public List<ModuleProgressModel> ModuleProgresses { get; set; } = new();
    }
}
