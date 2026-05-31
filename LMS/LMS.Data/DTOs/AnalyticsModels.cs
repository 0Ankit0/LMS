namespace LMS.Data.DTOs
{
    public class UserAnalyticsModel
    {
        public double OverallProgress { get; set; }
        public int CompletedCourses { get; set; }
        public int TotalStudyHours { get; set; }
        public double AverageScore { get; set; }
        public int TotalLessonsCompleted { get; set; }
        public int TotalModulesCompleted { get; set; }
        public int CurrentStreak { get; set; }
        public int ActiveDays { get; set; }
        public List<ActivityModel> RecentActivities { get; set; } = new();
        public List<MetricModel> PerformanceMetrics { get; set; } = new();
    }

    public class ActivityModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int? CourseId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class MetricModel
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double MaxValue { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class StudyTimeAnalytics
    {
        public Dictionary<DateTime, double> DailyStudyTime { get; set; } = new();
        public Dictionary<string, double> CourseStudyTime { get; set; } = new();
        public double TotalStudyTime { get; set; }
        public double AverageSessionLength { get; set; }
        public int TotalSessions { get; set; }
    }

    public class ProgressAnalytics
    {
        public Dictionary<DateTime, double> DailyProgress { get; set; } = new();
        public Dictionary<string, double> CourseProgress { get; set; } = new();
        public double OverallCompletionRate { get; set; }
        public double WeeklyProgressTrend { get; set; }
        public double MonthlyProgressTrend { get; set; }
    }

    public class PerformanceAnalytics
    {
        public Dictionary<string, double> QuizScores { get; set; } = new();
        public Dictionary<string, double> AssessmentScores { get; set; } = new();
        public double AverageQuizScore { get; set; }
        public double AverageAssessmentScore { get; set; }
        public List<string> StrongSubjects { get; set; } = new();
        public List<string> WeakSubjects { get; set; } = new();
    }
}
