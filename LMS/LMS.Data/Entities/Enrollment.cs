using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

        public double ProgressPercentage { get; set; } = 0;

        public TimeSpan TimeSpent { get; set; } = TimeSpan.Zero;

        public double? FinalGrade { get; set; }

        public bool IsCertificateIssued { get; set; }

        public DateTime? CertificateIssuedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<ModuleProgress> ModuleProgresses { get; set; } = new List<ModuleProgress>();
        public virtual ICollection<AssessmentAttempt> AssessmentAttempts { get; set; } = new List<AssessmentAttempt>();
    }

    public enum EnrollmentStatus
    {
        Active = 1,
        Completed = 2,
        Suspended = 3,
        Dropped = 4,
        Expired = 5
    }
}
