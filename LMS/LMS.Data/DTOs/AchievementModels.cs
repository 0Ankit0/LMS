namespace LMS.Data.DTOs
{
    public class CreateAchievementRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public string? IconName { get; set; }
        public int Points { get; set; }
        public string BadgeColor { get; set; } = "#3f51b5";
        public string Type { get; set; } = string.Empty;
        public List<AchievementCriteria> Criteria { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    public class UserAchievementsModel
    {
        public List<UserAchievementModel> EarnedAchievements { get; set; } = new();
        public List<AchievementModel> AvailableAchievements { get; set; } = new();
        public List<LeaderboardEntryModel> LeaderboardEntries { get; set; } = new();
        public int TotalPoints { get; set; }
        public int EarnedCount { get; set; }
        public int AvailableCount { get; set; }
        public int UserRank { get; set; }
    }

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
        public string BadgeColor { get; set; } = "#3f51b5";
        public string Type { get; set; } = string.Empty;
        public DateTime EarnedAt { get; set; }
        public bool IsVisible { get; set; } = true;
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
    }

    public class AchievementModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public string? IconName { get; set; } // Add for icon support
        public int Points { get; set; }
        public string BadgeColor { get; set; } = "#3f51b5";
        public string Type { get; set; } = string.Empty;
        public List<AchievementCriteria> Criteria { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int UsersEarnedCount { get; set; }
    }

    public class AchievementCriteria
    {
        public int Id { get; set; }
        public int AchievementId { get; set; }
        public CriteriaType Type { get; set; }
        public string? TargetValue { get; set; }
        public string? ComparisonOperator { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public enum CriteriaType
    {
        CourseCompletion,
        QuizScore,
        StudyTime,
        ConsecutiveDays,
        Participation,
        Social,
        Creation,
        Performance
    }

    public class LeaderboardEntryModel
    {
        public int Rank { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public double Score { get; set; }
        public int TotalPoints { get; set; }
        public int CompletedCourses { get; set; }
        public int Achievements { get; set; }
        public int AchievementCount { get; set; }
        public double AverageScore { get; set; }
        public int CurrentStreak { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class AchievementProgressModel
    {
        public int AchievementId { get; set; }
        public string AchievementName { get; set; } = string.Empty;
        public double CurrentProgress { get; set; }
        public double RequiredProgress { get; set; }
        public double ProgressPercentage => RequiredProgress > 0 ? (CurrentProgress / RequiredProgress) * 100 : 0;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
