using System;

namespace LMS.Data.DTOs
{
    // LMS Report DTOs
    public class StudentProgressReportDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public double ProgressPercentage { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public double? FinalGrade { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CompletedModules { get; set; }
        public int TotalModules { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedAssessments { get; set; }
        public int TotalAssessments { get; set; }
        public double AverageAssessmentScore { get; set; }
        public bool IsCertificateIssued { get; set; }
    }

    public class CourseCompletionReportDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int TotalEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public int ActiveEnrollments { get; set; }
        public int DroppedEnrollments { get; set; }
        public double CompletionRate { get; set; }
        public double AverageCompletionTime { get; set; } // in days
        public double AverageFinalGrade { get; set; }
        public int CertificatesIssued { get; set; }
        public DateTime CourseStartDate { get; set; }
        public DateTime? CourseEndDate { get; set; }
        public string CourseStatus { get; set; } = string.Empty;
    }

    public class AssessmentPerformanceReportDto
    {
        public int AssessmentId { get; set; }
        public string AssessmentTitle { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public int PassedAttempts { get; set; }
        public int FailedAttempts { get; set; }
        public double PassRate { get; set; }
        public double AverageScore { get; set; }
        public double HighestScore { get; set; }
        public double LowestScore { get; set; }
        public double AverageCompletionTime { get; set; } // in minutes
        public double PassingScore { get; set; }
        public int UniqueStudents { get; set; }
        public DateTime? LastAttemptDate { get; set; }
    }

    public class EnrollmentSummaryReportDto
    {
        public DateTime ReportDate { get; set; }
        public int TotalEnrollments { get; set; }
        public int NewEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public int ActiveEnrollments { get; set; }
        public int DroppedEnrollments { get; set; }
        public int SuspendedEnrollments { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string Period { get; set; } = string.Empty; // Daily, Weekly, Monthly
        public double AverageProgressPercentage { get; set; }
        public int CertificatesIssued { get; set; }
    }

    public class ForumActivityReportDto
    {
        public int ForumId { get; set; }
        public string ForumTitle { get; set; } = string.Empty;
        public string ForumName { get; set; } = string.Empty; // Added for compatibility
        public string CourseName { get; set; } = string.Empty;
        public int TotalTopics { get; set; }
        public int TotalPosts { get; set; }
        public int PostCount { get; set; } // Added for compatibility
        public int ReplyCount { get; set; } // Added for compatibility
        public int ActiveUsers { get; set; }
        public int UniqueParticipants { get; set; } // Added for compatibility
        public int TotalViews { get; set; }
        public int TotalViewCount { get; set; } // Added for compatibility
        public DateTime LastActivityDate { get; set; }
        public string MostActiveUser { get; set; } = string.Empty;
        public int MostActiveUserPosts { get; set; }
        public string MostPopularTopic { get; set; } = string.Empty;
        public int MostPopularTopicReplies { get; set; }
        public double AveragePostsPerUser { get; set; }
        public double AveragePostsPerDay { get; set; } // Added for compatibility
        public double AverageRepliesPerTopic { get; set; }
        public string TrendDirection { get; set; } = string.Empty; // Added for compatibility
        public int PeakActivityHour { get; set; } // Added for compatibility
    }

    // SIS Report DTOs
    public class StudentInformationReportDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }
        public int TotalCoursesEnrolled { get; set; }
        public int CompletedCourses { get; set; }
        public int ActiveCourses { get; set; }
        public double OverallGPA { get; set; }
        public int TotalCredits { get; set; }
        public int EarnedCredits { get; set; }
        public int TotalPoints { get; set; }
        public int Level { get; set; }
        public string Bio { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class AttendanceReportDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string AttendanceStatus { get; set; } = string.Empty; // Present, Absent, Late, Excused
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double AttendancePercentage { get; set; }
    }

    public class GradeDistributionReportDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int AGrades { get; set; }
        public int BGrades { get; set; }
        public int CGrades { get; set; }
        public int DGrades { get; set; }
        public int FGrades { get; set; }
        public double AverageGrade { get; set; }
        public double MedianGrade { get; set; }
        public double HighestGrade { get; set; }
        public double LowestGrade { get; set; }
        public double StandardDeviation { get; set; }
        public string GradeRange { get; set; } = string.Empty;
        public DateTime ReportGeneratedDate { get; set; }
    }

    public class TeacherPerformanceReportDto
    {
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalCoursesTeaching { get; set; }
        public int TotalStudentsTeaching { get; set; }
        public double AverageClassGrade { get; set; }
        public double StudentSatisfactionRating { get; set; }
        public int TotalAssignments { get; set; }
        public int GradedAssignments { get; set; }
        public double AverageGradingTime { get; set; } // in days
        public int ForumParticipation { get; set; }
        public int MessagesReplied { get; set; }
        public DateTime LastLoginDate { get; set; }
        public TimeSpan TotalTimeSpent { get; set; }
        public int CoursesCompleted { get; set; }
        public double CourseCompletionRate { get; set; }
        public string PerformanceStatus { get; set; } = string.Empty;
    }

    // Additional DTOs for endpoint compatibility
    public record ReportParametersDto(
        int? CourseId = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        string? UserId = null,
        string? InstructorId = null
    );

    public record CustomReportParametersDto(
        string ReportType,
        string? Format = "json",
        bool IncludeCharts = true
    );

    public class EnrollmentTrendReportDto
    {
        public DateTime Date { get; set; }
        public int NewEnrollments { get; set; }
        public int TotalEnrollments { get; set; }
        public int ActiveEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public int DroppedEnrollments { get; set; }
        public double EnrollmentRate { get; set; }
        public double CompletionRate { get; set; }
        public double RetentionRate { get; set; }
    }

    public class ReportDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public List<ChartData> Charts { get; set; } = new();
        public ReportMetadata Metadata { get; set; } = new();
    }

    public class ChartData
    {
        public string Type { get; set; } = string.Empty; // bar, line, pie, etc.
        public string Title { get; set; } = string.Empty;
        public List<string> Labels { get; set; } = new();
        public List<DataSeries> Series { get; set; } = new();
    }

    public class DataSeries
    {
        public string Name { get; set; } = string.Empty;
        public List<object> Data { get; set; } = new();
        public string? Color { get; set; }
    }

    public class ReportMetadata
    {
        public int TotalRecords { get; set; }
        public DateTime? DateRange_Start { get; set; }
        public DateTime? DateRange_End { get; set; }
        public Dictionary<string, object> Filters { get; set; } = new();
        public string GeneratedBy { get; set; } = string.Empty;
    }
}
