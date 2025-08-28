using LMS.Data.DTOs.LMS.Note;

namespace LMS.Data.DTOs
{
    public class DashboardDataModel
    {
        public CourseModel? ContinueCourse { get; set; }
        public List<ProgressCourseModel> ProgressCourses { get; set; } = new();
        public List<CourseModel> NewCourses { get; set; } = new();
        public List<CourseModel> YourCourses { get; set; } = new();
        public List<CourseModel> UpcomingCourses { get; set; } = new();
        public List<NoteQuickAccessDTO> UserNotes { get; set; } = new();
        public List<AnnouncementModel> RecentAnnouncements { get; set; } = new();
        public UserProgressSummary ProgressSummary { get; set; } = new();
    }

    public class ProgressCourseModel
    {
        public CourseModel Course { get; set; } = new();
        public double Progress { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public TimeSpan TimeSpent { get; set; }
    }

    public class UserProgressSummary
    {
        public double OverallProgress { get; set; }
        public int CompletedCourses { get; set; }
        public int TotalCourses { get; set; }
        public int TotalStudyHours { get; set; }
        public int CompletedAssessments { get; set; }
        public int TotalAssessments { get; set; }
        public int ActiveCourses { get; set; }
        public DateTime LastActivity { get; set; }
        public double AverageScore { get; set; }
        public int CurrentStreak { get; set; }
        public int ActiveDays { get; set; }
    }

    public enum AnnouncementType
    {
        General,
        Course,
        System,
        Emergency
    }

    public class AnnouncementModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public AnnouncementType Type { get; set; }
        public bool IsPinned { get; set; }
        public int? CourseId { get; set; }
        public string? CourseTitle { get; set; }
        public string? CourseName { get; set; }
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool SendEmail { get; set; }
        public bool SendSms { get; set; }
    }
}
