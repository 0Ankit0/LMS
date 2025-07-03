using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data
{
    // Gamification Models
    public class Achievement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public string? IconUrl { get; set; }

        public int Points { get; set; }

        public string BadgeColor { get; set; } = "#ffd700";

        public AchievementType Type { get; set; } = AchievementType.Course;

        public string? Criteria { get; set; } // JSON criteria for earning

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }

    public class UserAchievement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int AchievementId { get; set; }

        [ForeignKey("AchievementId")]
        public virtual Achievement Achievement { get; set; } = null!;

        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

        public int? CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
    }

    public class Leaderboard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public LeaderboardType Type { get; set; } = LeaderboardType.Points;

        public LeaderboardPeriod Period { get; set; } = LeaderboardPeriod.AllTime;

        public int? CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<LeaderboardEntry> Entries { get; set; } = new List<LeaderboardEntry>();
    }

    public class LeaderboardEntry
    {
        [Key]
        public int Id { get; set; }

        public int LeaderboardId { get; set; }

        [ForeignKey("LeaderboardId")]
        public virtual Leaderboard Leaderboard { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int Rank { get; set; }

        public double Score { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    // Additional Models for Resources and Analytics
    public class LessonResource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } = null!;

        public ResourceType Type { get; set; } = ResourceType.Document;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public string? ExternalUrl { get; set; }

        public long FileSize { get; set; }

        public string ContentType { get; set; } = string.Empty;

        public bool IsDownloadable { get; set; } = true;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    public class Certificate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [Required]
        public string CertificateNumber { get; set; } = string.Empty;

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public double FinalGrade { get; set; }

        public string? CertificateUrl { get; set; }

        public bool IsValid { get; set; } = true;
    }

    // Enums
    public enum AchievementType
    {
        Course = 1,
        Assessment = 2,
        Participation = 3,
        Time = 4,
        Streak = 5,
        Social = 6
    }

    public enum LeaderboardType
    {
        Points = 1,
        CourseCompletion = 2,
        AssessmentScores = 3,
        TimeSpent = 4,
        Achievements = 5
    }

    public enum LeaderboardPeriod
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Quarterly = 4,
        Yearly = 5,
        AllTime = 6
    }

    public enum ResourceType
    {
        Document = 1,
        Video = 2,
        Audio = 3,
        Image = 4,
        Link = 5,
        Archive = 6,
        Presentation = 7
    }
}
