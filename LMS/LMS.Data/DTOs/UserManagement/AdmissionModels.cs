namespace LMS.Data.DTOs.UserManagement
{
    public class AdmissionApplicationDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? EducationLevel { get; set; }
        public double? GPA { get; set; }
        public string? PreviousInstitution { get; set; }
        public string? FieldOfStudy { get; set; }
        public int? GraduationYear { get; set; }
        public string? PreferredProgram { get; set; }
        public string? IntendedStartDate { get; set; }
        public string StudyMode { get; set; } = "FullTime";
        public string? PersonalStatement { get; set; }
        public string Status { get; set; } = "Submitted";
        public DateTime? SubmittedAt { get; set; }
        public string? ReviewerNotes { get; set; }
        public List<ReviewEntryDto>? ReviewHistory { get; set; }
    }

    public class ApplicationDocumentDto
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string OriginalFileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string DocumentType { get; set; } = "";
        public DateTime UploadedAt { get; set; }
    }

    public class ProgramDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string? Department { get; set; }
        public int DurationMonths { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ReviewEntryDto
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string ReviewerName { get; set; } = "";
        public string ReviewerId { get; set; } = "";
        public DateTime ReviewDate { get; set; }
        public string Decision { get; set; } = "";
        public string? Notes { get; set; }
    }
}
