namespace LMS.Data.DTOs
{
    public class UserAchievementModel
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public int AchievementId { get; set; }

        public string AchievementName { get; set; } = string.Empty;

        public string AchievementDescription { get; set; } = string.Empty;

        public string? AchievementIconUrl { get; set; }

        public int Points { get; set; }

        public string BadgeColor { get; set; } = string.Empty;

        public DateTime EarnedAt { get; set; }

        public int? CourseId { get; set; }

        public string? CourseName { get; set; }
    }
}
