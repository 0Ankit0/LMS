using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs.UserManagement
{
    public class StudentProfileModel
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? StudentIdNumber { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public DateTime? GraduationDate { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public int TotalPoints { get; set; }
        public int Level { get; set; }

        // User details
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class InstructorProfileModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? OfficeHours { get; set; }
        public DateTime? HireDate { get; set; }

        // User details
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class ParentProfileModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? PreferredContactMethod { get; set; }

        // User details
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }

        // Linked students
        public List<StudentProfileModel> LinkedStudents { get; set; } = new();
    }

    public class CreateStudentProfileRequest
    {
        [Required]
        public DateTime DateOfBirth { get; set; }

        [StringLength(50)]
        public string? StudentIdNumber { get; set; }

        public DateTime? EnrollmentDate { get; set; }

        [StringLength(200)]
        public string? EmergencyContactName { get; set; }

        [StringLength(50)]
        public string? EmergencyContactPhone { get; set; }
    }

    public class CreateInstructorProfileRequest
    {
        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(500)]
        public string? OfficeHours { get; set; }

        public DateTime? HireDate { get; set; }
    }

    public class CreateParentProfileRequest
    {
        [StringLength(50)]
        public string? PreferredContactMethod { get; set; }
    }

    public class UpdateStudentProfileRequest
    {
        public DateTime? DateOfBirth { get; set; }

        [StringLength(50)]
        public string? StudentIdNumber { get; set; }

        public DateTime? EnrollmentDate { get; set; }

        public DateTime? GraduationDate { get; set; }

        [StringLength(200)]
        public string? EmergencyContactName { get; set; }

        [StringLength(50)]
        public string? EmergencyContactPhone { get; set; }
    }

    public class LinkParentStudentRequest
    {
        [Required]
        public string ParentId { get; set; } = string.Empty;

        [Required]
        public string StudentId { get; set; } = string.Empty;
    }

    public class UserSettingsModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "en-US";
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool PushNotifications { get; set; } = true;
    }

    public class UpdateUserSettingsRequest
    {
        [StringLength(50)]
        public string? Theme { get; set; }

        [StringLength(10)]
        public string? Language { get; set; }

        public bool? EmailNotifications { get; set; }
        public bool? SmsNotifications { get; set; }
        public bool? PushNotifications { get; set; }
    }

    public class UserActivityModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
    }
}
