using System;

namespace LMS.Web.Repositories.DTOs
{
    public class TeacherPerformanceReportDto
    {
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalCoursesTeaching { get; set; }
        public int TotalStudentsTeaching { get; set; }
        public double AverageClassGrade { get; set; }
        public double StudentSatisfactionRating { get; set; }
        public double CourseCompletionRate { get; set; }
        public int ForumParticipation { get; set; }
        public int MessagesReplied { get; set; }
        public string PerformanceStatus { get; set; } = string.Empty;
        public int TotalAssignments { get; set; }
        public int GradedAssignments { get; set; }
        public double AverageGradingTime { get; set; } // in days
        public TimeSpan TotalTimeSpent { get; set; }
        public int CoursesCompleted { get; set; }
        public DateTime LastLoginDate { get; set; }
    }
}