using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class UserModel
    {
        public string Id { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public DateTime DateOfBirth { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public int TotalPoints { get; set; }

        public int Level { get; set; }

        public List<string> Roles { get; set; } = new();

        public List<EnrollmentModel> Enrollments { get; set; } = new();

        public List<UserAchievementModel> Achievements { get; set; } = new();
    }
}
