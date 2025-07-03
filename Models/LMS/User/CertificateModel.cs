namespace LMS.Models.User
{
    public class CertificateModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public double FinalGrade { get; set; }
        public string? CertificateUrl { get; set; }
        public bool IsValid { get; set; }
    }

    public class CreateCertificateRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public double FinalGrade { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
