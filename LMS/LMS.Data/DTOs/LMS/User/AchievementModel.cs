using LMS.Data.Entities;

namespace LMS.Data.DTOs
{
    public class AchievementModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public int Points { get; set; }
        public string BadgeColor { get; set; } = "#ffd700";
        public string Type { get; set; } = string.Empty;
        public List<AchievementCriteria> Criteria { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UsersEarnedCount { get; set; }
    }

    public class CreateAchievementRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public int Points { get; set; }
        public string BadgeColor { get; set; } = "#ffd700";
        public string Type { get; set; } = "Course";
        public List<AchievementCriteria> Criteria { get; set; } = new();
    }
}
