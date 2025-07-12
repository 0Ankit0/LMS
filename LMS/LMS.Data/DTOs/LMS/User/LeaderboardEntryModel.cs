namespace LMS.Data.DTOs
{
    public class LeaderboardEntryModel
    {
        public int Rank { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        public double Score { get; set; }

        public double TotalPoints { get; set; }

        public int CompletedCourses { get; set; }

        public int Achievements { get; set; }

        public int AchievementCount { get; set; }

        public double AverageScore { get; set; }

        public int CurrentStreak { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
