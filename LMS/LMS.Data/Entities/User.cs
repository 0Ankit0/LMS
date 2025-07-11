using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LMS.Data.Entities
{
    public class User : IdentityUser
    {
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        [StringLength(500)]
        public string? Bio { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public DateTime DateOfBirth { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        // Gamification
        public int TotalPoints { get; set; }
        public int Level { get; set; } = 1;

        // Navigation Properties
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
        public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public virtual ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
        public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }
}
