using System;

namespace LMS.Web.Repositories.DTOs
{
    public class StudentInformationReportDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }
        public int TotalCoursesEnrolled { get; set; }
        public int CompletedCourses { get; set; }
        public int ActiveCourses { get; set; }
        public double OverallGPA { get; set; }
        public int TotalCredits { get; set; }
        public int EarnedCredits { get; set; }
        public int TotalPoints { get; set; }
        public int Level { get; set; }
        public string Bio { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}