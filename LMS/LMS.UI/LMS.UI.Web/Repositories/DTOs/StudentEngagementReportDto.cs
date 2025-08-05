namespace LMS.Web.Repositories.DTOs
{
    public class StudentEngagementReportDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int TotalForumPosts { get; set; }
        public int TopicsCreated { get; set; }
        public int RepliesMade { get; set; }
        public int TotalCoursesEnrolled { get; set; }
        public int CompletedCourses { get; set; }
        public TimeSpan TimeSpentOnPlatform { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string LastForumActivity { get; set; } = string.Empty;
        public double AverageAssessmentScore { get; set; }
        public double ProgressPercentage { get; set; }
        public List<CourseProgressDto> CourseProgress { get; set; } = new();

        public class CourseProgressDto
        {
            public string CourseName { get; set; } = string.Empty;
            public double Progress { get; set; }
        }
    }
}
