using LMS.Data.Entities;
using System.ComponentModel.DataAnnotations;

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
        public string? IconName { get; set; } // Add for icon support
    }

    public class CreateAchievementRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public int Points { get; set; }
        public string BadgeColor { get; set; } = "#ffd700";
        public string Type { get; set; } = "Course";
        public List<AchievementCriteria> Criteria { get; set; } = new();
        public string? IconName { get; set; } // Add for icon support
    }
}
