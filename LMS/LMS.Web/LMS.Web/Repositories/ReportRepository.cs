using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LMS.Web.Repositories.DTOs;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LMS.Data.Entities;

namespace LMS.Repositories
{
   public interface IReportRepository
{
    // LMS Reports
    Task<IEnumerable<StudentProgressReportDto>> GetStudentProgressReportAsync(User user, string? studentId = null);
    Task<IEnumerable<StudentProgressReportDto>> GetStudentProgressByCoursesAsync(User user, string studentId);
    Task<CourseCompletionReportDto> GetCourseCompletionReportAsync(User user, int courseId);
    Task<IEnumerable<CourseCompletionReportDto>> GetAllCoursesCompletionReportAsync(User user);
    Task<AssessmentPerformanceReportDto> GetAssessmentPerformanceReportAsync(User user, int assessmentId);
    Task<IEnumerable<AssessmentPerformanceReportDto>> GetCourseAssessmentsPerformanceReportAsync(User user, int courseId);
    Task<IEnumerable<EnrollmentSummaryReportDto>> GetEnrollmentSummaryReportAsync(User user, DateTime startDate, DateTime endDate, string period = "Daily");
    Task<IEnumerable<EnrollmentSummaryReportDto>> GetEnrollmentTrendsReportAsync(User user, int months = 12);
    Task<ForumActivityReportDto> GetForumActivityReportAsync(User user, int forumId);
    Task<IEnumerable<ForumActivityReportDto>> GetAllForumsActivityReportAsync(User user);

    // Advanced LMS Reports
    Task<IEnumerable<StudentProgressReportDto>> GetLowPerformanceStudentsReportAsync(User user, double threshold = 50.0);
    Task<IEnumerable<CourseCompletionReportDto>> GetPopularCoursesReportAsync(User user, int topN = 10);
    Task<IEnumerable<AssessmentPerformanceReportDto>> GetDifficultAssessmentsReportAsync(User user, double passRateThreshold = 70.0);
    Task<object> GetLearningAnalyticsReportAsync(User user, int courseId);
    Task<object> GetStudentEngagementReportAsync(User user, string studentId);

    // SIS Reports
    Task<StudentInformationReportDto> GetStudentInformationReportAsync(User user, string studentId);
    Task<IEnumerable<StudentInformationReportDto>> GetAllStudentsInformationReportAsync(User user);
    Task<IEnumerable<StudentInformationReportDto>> GetStudentsByStatusReportAsync(User user, string status);
    Task<IEnumerable<AttendanceReportDto>> GetAttendanceReportAsync(User user, int classId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<AttendanceReportDto>> GetStudentAttendanceReportAsync(User user, string studentId, DateTime startDate, DateTime endDate);
    Task<GradeDistributionReportDto> GetGradeDistributionReportAsync(User user, int classId);
    Task<IEnumerable<GradeDistributionReportDto>> GetAllClassesGradeDistributionReportAsync(User user);
    Task<TeacherPerformanceReportDto> GetTeacherPerformanceReportAsync(User user, string teacherId);
    Task<IEnumerable<TeacherPerformanceReportDto>> GetAllTeachersPerformanceReportAsync(User user);

    // Advanced SIS Reports
    Task<object> GetAcademicPerformanceTrendsReportAsync(User user, string studentId, int months = 12);
    Task<IEnumerable<object>> GetRiskStudentsReportAsync(User user);
    Task<object> GetInstitutionalEffectivenessReportAsync(User user);
    Task<object> GetResourceUtilizationReportAsync(User user);
    Task<object> GetRetentionAnalysisReportAsync(User user);

    // Export Reports
    Task<byte[]> ExportReportToPdfAsync(User user, string reportType, object parameters);
    Task<byte[]> ExportReportToExcelAsync(User user, string reportType, object parameters);
    Task<string> ExportReportToCsvAsync(User user, string reportType, object parameters);
}

    public class ReportRepository : IReportRepository
    {
        private readonly ILogger<ReportRepository> _logger;
        private readonly ApplicationDbContext _context;

        public ReportRepository(ILogger<ReportRepository> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        #region LMS Reports

        public async Task<IEnumerable<StudentProgressReportDto>> GetStudentProgressReportAsync(User user,string? studentId = null)
        {
            try
            {

                var enrollmentsQuery = _context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Course)
                    .Include(e => e.AssessmentAttempts)
                    .Include(e => e.ModuleProgresses)
                        .ThenInclude(mp => mp.Module)
                            .ThenInclude(m => m.Lessons)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(studentId))
                {
                    enrollmentsQuery = enrollmentsQuery.Where(e => e.UserId == studentId);
                }

                var enrollments = await enrollmentsQuery.ToListAsync();
                var reports = new List<StudentProgressReportDto>();

                foreach (var enrollment in enrollments)
                {
                    var totalModules = enrollment.Course.Modules.Count;
                    var completedModules = enrollment.ModuleProgresses.Count(mp => mp.IsCompleted);

                    var totalLessons = enrollment.Course.Modules.SelectMany(m => m.Lessons).Count();
                    var completedLessons = enrollment.ModuleProgresses
                        .SelectMany(mp => mp.LessonProgresses)
                        .Count(lp => lp.IsCompleted);

                    var assessments = await _context.Assessments
                        .Where(a => a.CourseId == enrollment.CourseId)
                        .CountAsync();

                    var completedAssessments = enrollment.AssessmentAttempts
                        .Where(aa => aa.Status == AssessmentAttemptStatus.Completed && aa.IsPassed)
                        .Select(aa => aa.AssessmentId)
                        .Distinct()
                        .Count();

                    var averageScore = enrollment.AssessmentAttempts
                        .Where(aa => aa.Status == AssessmentAttemptStatus.Completed && aa.Score.HasValue)
                        .Select(aa => aa.Score!.Value)
                        .DefaultIfEmpty(0)
                        .Average();

                    reports.Add(new StudentProgressReportDto
                    {
                        StudentId = enrollment.User.Id,
                        StudentName = enrollment.User.FullName,
                        Email = enrollment.User.Email ?? "",
                        CourseName = enrollment.Course.Title,
                        CourseId = enrollment.CourseId,
                        ProgressPercentage = enrollment.ProgressPercentage,
                        TimeSpent = enrollment.TimeSpent,
                        EnrolledAt = enrollment.EnrolledAt,
                        StartedAt = enrollment.StartedAt,
                        CompletedAt = enrollment.CompletedAt,
                        FinalGrade = enrollment.FinalGrade,
                        Status = enrollment.Status.ToString(),
                        CompletedModules = completedModules,
                        TotalModules = totalModules,
                        CompletedLessons = completedLessons,
                        TotalLessons = totalLessons,
                        CompletedAssessments = completedAssessments,
                        TotalAssessments = assessments,
                        AverageAssessmentScore = averageScore,
                        IsCertificateIssued = enrollment.IsCertificateIssued
                    });
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating student progress report for student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<IEnumerable<StudentProgressReportDto>> GetStudentProgressByCoursesAsync(User user,string studentId)
        {
            try
            {

                var enrollments = await _context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Course)
                    .Include(e => e.AssessmentAttempts)
                    .Include(e => e.ModuleProgresses)
                        .ThenInclude(mp => mp.Module)
                            .ThenInclude(m => m.Lessons)
                    .Where(e => e.UserId == studentId)
                    .ToListAsync();

                var reports = new List<StudentProgressReportDto>();

                foreach (var enrollment in enrollments)
                {
                    var totalModules = enrollment.Course.Modules.Count;
                    var completedModules = enrollment.ModuleProgresses.Count(mp => mp.IsCompleted);

                    var totalLessons = enrollment.Course.Modules.SelectMany(m => m.Lessons).Count();
                    var completedLessons = enrollment.ModuleProgresses
                        .SelectMany(mp => mp.LessonProgresses)
                        .Count(lp => lp.IsCompleted);

                    var assessments = await _context.Assessments
                        .Where(a => a.CourseId == enrollment.CourseId)
                        .CountAsync();

                    var completedAssessments = enrollment.AssessmentAttempts
                        .Where(aa => aa.Status == AssessmentAttemptStatus.Completed && aa.IsPassed)
                        .Select(aa => aa.AssessmentId)
                        .Distinct()
                        .Count();

                    var averageScore = enrollment.AssessmentAttempts
                        .Where(aa => aa.Status == AssessmentAttemptStatus.Completed && aa.Score.HasValue)
                        .Select(aa => aa.Score!.Value)
                        .DefaultIfEmpty(0)
                        .Average();

                    reports.Add(new StudentProgressReportDto
                    {
                        StudentId = enrollment.User.Id,
                        StudentName = enrollment.User.FullName,
                        Email = enrollment.User.Email ?? "",
                        CourseName = enrollment.Course.Title,
                        CourseId = enrollment.CourseId,
                        ProgressPercentage = enrollment.ProgressPercentage,
                        TimeSpent = enrollment.TimeSpent,
                        EnrolledAt = enrollment.EnrolledAt,
                        StartedAt = enrollment.StartedAt,
                        CompletedAt = enrollment.CompletedAt,
                        FinalGrade = enrollment.FinalGrade,
                        Status = enrollment.Status.ToString(),
                        CompletedModules = completedModules,
                        TotalModules = totalModules,
                        CompletedLessons = completedLessons,
                        TotalLessons = totalLessons,
                        CompletedAssessments = completedAssessments,
                        TotalAssessments = assessments,
                        AverageAssessmentScore = averageScore,
                        IsCertificateIssued = enrollment.IsCertificateIssued
                    });
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating course progress report for student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<CourseCompletionReportDto> GetCourseCompletionReportAsync(User user,int courseId)
        {
            try
            {

                var course = await _context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    throw new ArgumentException($"Course with ID {courseId} not found");
                }

                var totalEnrollments = course.Enrollments.Count;
                var completedEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);
                var activeEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active);
                var droppedEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Dropped);

                var averageCompletionTime = course.Enrollments
                    .Where(e => e.CompletedAt.HasValue && e.StartedAt.HasValue)
                    .Select(e => (e.CompletedAt!.Value - e.StartedAt!.Value).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average();

                return new CourseCompletionReportDto
                {
                    CourseId = course.Id,
                    CourseName = course.Title,
                    InstructorName = course.Instructor?.FullName ?? "Unknown",
                    TotalEnrollments = totalEnrollments,
                    CompletedEnrollments = completedEnrollments,
                    ActiveEnrollments = activeEnrollments,
                    DroppedEnrollments = droppedEnrollments,
                    CompletionRate = totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments * 100 : 0,
                    AverageCompletionTime = averageCompletionTime,
                    AverageFinalGrade = course.Enrollments.Where(e => e.FinalGrade.HasValue).Select(e => e.FinalGrade!.Value).DefaultIfEmpty(0).Average(),
                    CertificatesIssued = course.Enrollments.Count(e => e.IsCertificateIssued),
                    CourseStartDate = course.StartDate,
                    CourseEndDate = course.EndDate,
                    CourseStatus = course.Status.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating course completion report for course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<IEnumerable<CourseCompletionReportDto>> GetAllCoursesCompletionReportAsync(User user)
        {
            try
            {

                var courses = await _context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .Where(c => c.IsActive)
                    .ToListAsync();

                var reports = new List<CourseCompletionReportDto>();

                foreach (var course in courses)
                {
                    var totalEnrollments = course.Enrollments.Count;
                    var completedEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);
                    var activeEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active);
                    var droppedEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Dropped);
                    var averageCompletionTime = course.Enrollments
                        .Where(e => e.CompletedAt.HasValue && e.StartedAt.HasValue)
                        .Select(e => (e.CompletedAt!.Value - e.StartedAt!.Value).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average();

                    reports.Add(new CourseCompletionReportDto
                    {
                        CourseId = course.Id,
                        CourseName = course.Title,
                        InstructorName = course.Instructor?.FullName ?? "Unknown",
                        TotalEnrollments = totalEnrollments,
                        CompletedEnrollments = completedEnrollments,
                        ActiveEnrollments = activeEnrollments,
                        DroppedEnrollments = droppedEnrollments,
                        CompletionRate = totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments * 100 : 0,
                        AverageCompletionTime = averageCompletionTime,
                        AverageFinalGrade = course.Enrollments.Where(e => e.FinalGrade.HasValue).Select(e => e.FinalGrade!.Value).DefaultIfEmpty(0).Average(),
                        CertificatesIssued = course.Enrollments.Count(e => e.IsCertificateIssued),
                        CourseStartDate = course.StartDate,
                        CourseEndDate = course.EndDate,
                        CourseStatus = course.Status.ToString()
                    });
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating all courses completion report");
                throw;
            }
        }

        public async Task<AssessmentPerformanceReportDto> GetAssessmentPerformanceReportAsync(User user,int assessmentId)
        {
            try
            {

                var assessment = await _context.Assessments
                    .Include(a => a.Course)
                    .Include(a => a.Attempts)
                    .FirstOrDefaultAsync(a => a.Id == assessmentId);

                if (assessment == null)
                {
                    throw new ArgumentException($"Assessment with ID {assessmentId} not found");
                }

                var attempts = assessment.Attempts.Where(a => a.Status == AssessmentAttemptStatus.Completed).ToList();
                var totalAttempts = attempts.Count;
                var passedAttempts = attempts.Count(a => a.IsPassed);
                var failedAttempts = totalAttempts - passedAttempts;
                var scores = attempts.Where(a => a.Score.HasValue).Select(a => a.Score!.Value).ToList();

                var uniqueStudents = attempts.Select(a => a.Enrollment.UserId).Distinct().Count();
                var completionTimes = attempts.Where(a => a.TimeTaken.HasValue).Select(a => a.TimeTaken!.Value.TotalMinutes).ToList();

                return new AssessmentPerformanceReportDto
                {
                    AssessmentId = assessment.Id,
                    AssessmentTitle = assessment.Title,
                    AssessmentType = assessment.Type.ToString(),
                    CourseName = assessment.Course?.Title ?? "N/A",
                    TotalAttempts = totalAttempts,
                    PassedAttempts = passedAttempts,
                    FailedAttempts = failedAttempts,
                    PassRate = totalAttempts > 0 ? (double)passedAttempts / totalAttempts * 100 : 0,
                    AverageScore = scores.DefaultIfEmpty(0).Average(),
                    HighestScore = scores.DefaultIfEmpty(0).Max(),
                    LowestScore = scores.DefaultIfEmpty(0).Min(),
                    AverageCompletionTime = completionTimes.DefaultIfEmpty(0).Average(),
                    PassingScore = assessment.PassingScore,
                    UniqueStudents = uniqueStudents,
                    LastAttemptDate = attempts.OrderByDescending(a => a.CompletedAt).FirstOrDefault()?.CompletedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating assessment performance report for assessment: {AssessmentId}", assessmentId);
                throw;
            }
        }

        public async Task<IEnumerable<AssessmentPerformanceReportDto>> GetCourseAssessmentsPerformanceReportAsync(User user,int courseId)
        {
            try
            {

                var assessments = await _context.Assessments
                    .Include(a => a.Course)
                    .Include(a => a.Attempts)
                    .Where(a => a.CourseId == courseId)
                    .ToListAsync();

                var reports = new List<AssessmentPerformanceReportDto>();

                foreach (var assessment in assessments)
                {
                    var attempts = assessment.Attempts.Where(a => a.Status == AssessmentAttemptStatus.Completed).ToList();
                    var totalAttempts = attempts.Count;
                    var passedAttempts = attempts.Count(a => a.IsPassed);
                    var scores = attempts.Where(a => a.Score.HasValue).Select(a => a.Score!.Value).ToList();
                    var uniqueStudents = attempts.Select(a => a.Enrollment.UserId).Distinct().Count();
                    var completionTimes = attempts.Where(a => a.TimeTaken.HasValue).Select(a => a.TimeTaken!.Value.TotalMinutes).ToList();

                    reports.Add(new AssessmentPerformanceReportDto
                    {
                        AssessmentId = assessment.Id,
                        AssessmentTitle = assessment.Title,
                        AssessmentType = assessment.Type.ToString(),
                        CourseName = assessment.Course?.Title ?? "N/A",
                        TotalAttempts = totalAttempts,
                        PassedAttempts = passedAttempts,
                        FailedAttempts = totalAttempts - passedAttempts,
                        PassRate = totalAttempts > 0 ? (double)passedAttempts / totalAttempts * 100 : 0,
                        AverageScore = scores.DefaultIfEmpty(0).Average(),
                        HighestScore = scores.DefaultIfEmpty(0).Max(),
                        LowestScore = scores.DefaultIfEmpty(0).Min(),
                        AverageCompletionTime = completionTimes.DefaultIfEmpty(0).Average(),
                        PassingScore = assessment.PassingScore,
                        UniqueStudents = uniqueStudents,
                        LastAttemptDate = attempts.OrderByDescending(a => a.CompletedAt).FirstOrDefault()?.CompletedAt
                    });
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating course assessments performance report for course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<IEnumerable<EnrollmentSummaryReportDto>> GetEnrollmentSummaryReportAsync(User user,DateTime startDate, DateTime endDate, string period = "Daily")
        {
            try
            {

                var enrollments = await _context.Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.EnrolledAt >= startDate && e.EnrolledAt <= endDate)
                    .ToListAsync();

                var reports = new List<EnrollmentSummaryReportDto>();

                if (period.ToLower() == "daily")
                {
                    var groupedByDay = enrollments
                        .GroupBy(e => e.EnrolledAt.Date)
                        .OrderBy(g => g.Key);

                    foreach (var group in groupedByDay)
                    {
                        var dayEnrollments = group.ToList();
                        reports.Add(CreateEnrollmentSummaryReport(group.Key, dayEnrollments, period));
                    }
                }
                else if (period.ToLower() == "weekly")
                {
                    var groupedByWeek = enrollments
                        .GroupBy(e => GetWeekStart(e.EnrolledAt))
                        .OrderBy(g => g.Key);

                    foreach (var group in groupedByWeek)
                    {
                        var weekEnrollments = group.ToList();
                        reports.Add(CreateEnrollmentSummaryReport(group.Key, weekEnrollments, period));
                    }
                }
                else if (period.ToLower() == "monthly")
                {
                    var groupedByMonth = enrollments
                        .GroupBy(e => new DateTime(e.EnrolledAt.Year, e.EnrolledAt.Month, 1))
                        .OrderBy(g => g.Key);

                    foreach (var group in groupedByMonth)
                    {
                        var monthEnrollments = group.ToList();
                        reports.Add(CreateEnrollmentSummaryReport(group.Key, monthEnrollments, period));
                    }
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating enrollment summary report from {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        private static DateTime GetWeekStart(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private static EnrollmentSummaryReportDto CreateEnrollmentSummaryReport(DateTime reportDate, List<Enrollment> enrollments, string period)
        {
            var totalEnrollments = enrollments.Count;
            var completedEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Completed);
            var activeEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Active);
            var droppedEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Dropped);
            var suspendedEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Suspended);
            var certificatesIssued = enrollments.Count(e => e.IsCertificateIssued);
            var averageProgress = enrollments.DefaultIfEmpty().Average(e => e?.ProgressPercentage ?? 0);

            return new EnrollmentSummaryReportDto
            {
                ReportDate = reportDate,
                TotalEnrollments = totalEnrollments,
                NewEnrollments = totalEnrollments, // Since we're filtering by enrollment date
                CompletedEnrollments = completedEnrollments,
                ActiveEnrollments = activeEnrollments,
                DroppedEnrollments = droppedEnrollments,
                SuspendedEnrollments = suspendedEnrollments,
                Period = period,
                AverageProgressPercentage = averageProgress,
                CertificatesIssued = certificatesIssued,
                CourseName = "All Courses",
                CourseId = 0
            };
        }

        public async Task<IEnumerable<EnrollmentSummaryReportDto>> GetEnrollmentTrendsReportAsync(User user,int months = 12)
        {
            try
            {
                // Get enrollment trends over specified number of months
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddMonths(-months);

                return await GetEnrollmentSummaryReportAsync(user,startDate, endDate, "Monthly");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating enrollment trends report for {Months} months", months);
                throw;
            }
        }

        public async Task<ForumActivityReportDto> GetForumActivityReportAsync(User user,int forumId)
        {
            try
            {

                var forum = await _context.Forums
                    .Include(f => f.Course)
                    .Include(f => f.Topics)
                        .ThenInclude(t => t.Posts)
                            .ThenInclude(p => p.Author)
                    .FirstOrDefaultAsync(f => f.Id == forumId);

                if (forum == null)
                {
                    throw new ArgumentException($"Forum with ID {forumId} not found");
                }

                var allPosts = forum.Topics.SelectMany(t => t.Posts).ToList();
                var totalTopics = forum.Topics.Count;
                var totalPosts = allPosts.Count;
                var totalViews = forum.Topics.Count; // Since ViewCount is not available, use topic count as approximation
                var activeUsers = allPosts.Select(p => p.AuthorId).Distinct().Count();

                var lastActivity = allPosts.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue;

                var mostActiveUser = allPosts
                    .GroupBy(p => new { p.AuthorId, p.Author.FullName })
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                var mostPopularTopic = forum.Topics
                    .OrderByDescending(t => t.Posts.Count)
                    .FirstOrDefault();

                return new ForumActivityReportDto
                {
                    ForumId = forum.Id,
                    ForumTitle = forum.Title,
                    CourseName = forum.Course?.Title ?? "General Forum",
                    TotalTopics = totalTopics,
                    TotalPosts = totalPosts,
                    ActiveUsers = activeUsers,
                    TotalViews = totalViews,
                    LastActivityDate = lastActivity,
                    MostActiveUser = mostActiveUser?.Key.FullName ?? "N/A",
                    MostActiveUserPosts = mostActiveUser?.Count() ?? 0,
                    MostPopularTopic = mostPopularTopic?.Title ?? "N/A",
                    MostPopularTopicReplies = mostPopularTopic?.Posts.Count ?? 0,
                    AveragePostsPerUser = activeUsers > 0 ? (double)totalPosts / activeUsers : 0,
                    AverageRepliesPerTopic = totalTopics > 0 ? (double)totalPosts / totalTopics : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating forum activity report for forum: {ForumId}", forumId);
                throw;
            }
        }

        public async Task<IEnumerable<ForumActivityReportDto>> GetAllForumsActivityReportAsync(User user)
        {
            try
            {

                var forums = await _context.Forums
                    .Include(f => f.Course)
                    .Include(f => f.Topics)
                        .ThenInclude(t => t.Posts)
                            .ThenInclude(p => p.Author)
                    .Where(f => f.IsActive)
                    .ToListAsync();

                var reports = new List<ForumActivityReportDto>();

                foreach (var forum in forums)
                {
                    var allPosts = forum.Topics.SelectMany(t => t.Posts).ToList();
                    var totalTopics = forum.Topics.Count;
                    var totalPosts = allPosts.Count;
                    var totalViews = forum.Topics.Count; // Approximation
                    var activeUsers = allPosts.Select(p => p.AuthorId).Distinct().Count();

                    var lastActivity = allPosts.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue;

                    var mostActiveUser = allPosts
                        .GroupBy(p => new { p.AuthorId, p.Author.FullName })
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault();

                    var mostPopularTopic = forum.Topics
                        .OrderByDescending(t => t.Posts.Count)
                        .FirstOrDefault();

                    reports.Add(new ForumActivityReportDto
                    {
                        ForumId = forum.Id,
                        ForumTitle = forum.Title,
                        CourseName = forum.Course?.Title ?? "General Forum",
                        TotalTopics = totalTopics,
                        TotalPosts = totalPosts,
                        ActiveUsers = activeUsers,
                        TotalViews = totalViews,
                        LastActivityDate = lastActivity,
                        MostActiveUser = mostActiveUser?.Key.FullName ?? "N/A",
                        MostActiveUserPosts = mostActiveUser?.Count() ?? 0,
                        MostPopularTopic = mostPopularTopic?.Title ?? "N/A",
                        MostPopularTopicReplies = mostPopularTopic?.Posts.Count ?? 0,
                        AveragePostsPerUser = activeUsers > 0 ? (double)totalPosts / activeUsers : 0,
                        AverageRepliesPerTopic = totalTopics > 0 ? (double)totalPosts / totalTopics : 0
                    });
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating all forums activity report");
                throw;
            }
        }

        public async Task<IEnumerable<StudentProgressReportDto>> GetLowPerformanceStudentsReportAsync(User user,double threshold = 50.0)
        {
            try
            {

                var enrollments = await _context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Course)
                    .Include(e => e.AssessmentAttempts)
                    .Where(e => e.ProgressPercentage < threshold ||
                               (e.FinalGrade.HasValue && e.FinalGrade.Value < threshold))
                    .ToListAsync();

                var reports = new List<StudentProgressReportDto>();

                foreach (var enrollment in enrollments)
                {
                    var averageScore = enrollment.AssessmentAttempts
                        .Where(aa => aa.Status == AssessmentAttemptStatus.Completed && aa.Score.HasValue)
                        .Select(aa => aa.Score!.Value)
                        .DefaultIfEmpty(0)
                        .Average();

                    reports.Add(new StudentProgressReportDto
                    {
                        StudentId = enrollment.User.Id,
                        StudentName = enrollment.User.FullName,
                        Email = enrollment.User.Email ?? "",
                        CourseName = enrollment.Course.Title,
                        CourseId = enrollment.CourseId,
                        ProgressPercentage = enrollment.ProgressPercentage,
                        TimeSpent = enrollment.TimeSpent,
                        EnrolledAt = enrollment.EnrolledAt,
                        StartedAt = enrollment.StartedAt,
                        CompletedAt = enrollment.CompletedAt,
                        FinalGrade = enrollment.FinalGrade,
                        Status = enrollment.Status.ToString(),
                        AverageAssessmentScore = averageScore,
                        IsCertificateIssued = enrollment.IsCertificateIssued
                    });
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating low performance students report with threshold: {Threshold}", threshold);
                throw;
            }
        }

        public async Task<IEnumerable<CourseCompletionReportDto>> GetPopularCoursesReportAsync(User user,int topN = 10)
        {
            try
            {

                var courses = await _context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.Enrollments.Count)
                    .Take(topN)
                    .ToListAsync();

                var reports = new List<CourseCompletionReportDto>();

                foreach (var course in courses)
                {
                    var totalEnrollments = course.Enrollments.Count;
                    var completedEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);
                    var completionRate = totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments * 100 : 0;

                    reports.Add(new CourseCompletionReportDto
                    {
                        CourseId = course.Id,
                        CourseName = course.Title,
                        InstructorName = course.Instructor?.FullName ?? "Unknown",
                        TotalEnrollments = totalEnrollments,
                        CompletedEnrollments = completedEnrollments,
                        ActiveEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                        DroppedEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Dropped),
                        CompletionRate = completionRate,
                        AverageFinalGrade = course.Enrollments.Where(e => e.FinalGrade.HasValue).Select(e => e.FinalGrade!.Value).DefaultIfEmpty(0).Average(),
                        CertificatesIssued = course.Enrollments.Count(e => e.IsCertificateIssued),
                        CourseStartDate = course.StartDate,
                        CourseEndDate = course.EndDate,
                        CourseStatus = course.Status.ToString()
                    });
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating popular courses report for top {TopN}", topN);
                throw;
            }
        }

        public async Task<IEnumerable<AssessmentPerformanceReportDto>> GetDifficultAssessmentsReportAsync(User user,double passRateThreshold = 70.0)
        {
            try
            {

                var assessments = await _context.Assessments
                    .Include(a => a.Course)
                    .Include(a => a.Attempts)
                    .ToListAsync();

                var reports = new List<AssessmentPerformanceReportDto>();

                foreach (var assessment in assessments)
                {
                    var attempts = assessment.Attempts.Where(a => a.Status == AssessmentAttemptStatus.Completed).ToList();
                    if (attempts.Count == 0) continue;

                    var passedAttempts = attempts.Count(a => a.IsPassed);
                    var passRate = (double)passedAttempts / attempts.Count * 100;

                    if (passRate < passRateThreshold)
                    {
                        var scores = attempts.Where(a => a.Score.HasValue).Select(a => a.Score!.Value).ToList();
                        var uniqueStudents = attempts.Select(a => a.Enrollment.UserId).Distinct().Count();
                        var completionTimes = attempts.Where(a => a.TimeTaken.HasValue).Select(a => a.TimeTaken!.Value.TotalMinutes).ToList();

                        reports.Add(new AssessmentPerformanceReportDto
                        {
                            AssessmentId = assessment.Id,
                            AssessmentTitle = assessment.Title,
                            AssessmentType = assessment.Type.ToString(),
                            CourseName = assessment.Course?.Title ?? "N/A",
                            TotalAttempts = attempts.Count,
                            PassedAttempts = passedAttempts,
                            FailedAttempts = attempts.Count - passedAttempts,
                            PassRate = passRate,
                            AverageScore = scores.DefaultIfEmpty(0).Average(),
                            HighestScore = scores.DefaultIfEmpty(0).Max(),
                            LowestScore = scores.DefaultIfEmpty(0).Min(),
                            AverageCompletionTime = completionTimes.DefaultIfEmpty(0).Average(),
                            PassingScore = assessment.PassingScore,
                            UniqueStudents = uniqueStudents,
                            LastAttemptDate = attempts.OrderByDescending(a => a.CompletedAt).FirstOrDefault()?.CompletedAt
                        });
                    }
                }

                return reports.OrderBy(r => r.PassRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating difficult assessments report with threshold: {Threshold}", passRateThreshold);
                throw;
            }
        }

        public async Task<object> GetLearningAnalyticsReportAsync(User user,int courseId)
        {
            try
            {

                var course = await _context.Courses
                    .Include(c => c.Enrollments)
                        .ThenInclude(e => e.ModuleProgresses)
                    .Include(c => c.Modules)
                        .ThenInclude(m => m.Lessons)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    return new { Message = "Course not found" };
                }

                var analytics = new
                {
                    CourseId = courseId,
                    CourseName = course.Title,
                    TotalStudents = course.Enrollments.Count,
                    AverageProgress = course.Enrollments.Average(e => e.ProgressPercentage),
                    CompletionRate = course.Enrollments.Count > 0 ?
                        (double)course.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed) / course.Enrollments.Count * 100 : 0,
                    AverageTimeSpent = course.Enrollments.Average(e => e.TimeSpent.TotalHours),
                    PopularModules = course.Modules.Select(m => new
                    {
                        ModuleId = m.Id,
                        ModuleName = m.Title,
                        CompletionRate = course.Enrollments.Count > 0 ?
                            course.Enrollments.Count(e => e.ModuleProgresses.Any(mp => mp.ModuleId == m.Id && mp.IsCompleted)) / (double)course.Enrollments.Count * 100 : 0
                    }).OrderByDescending(m => m.CompletionRate).Take(5)
                };

                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating learning analytics report for course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<object> GetStudentEngagementReportAsync(User user,string studentId)
        {
            try
            {

                var student = await _context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .Include(u => u.ForumPosts)
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.AssessmentAttempts)
                    .FirstOrDefaultAsync(u => u.Id == studentId);

                if (student == null)
                {
                    return new { Message = "Student not found" };
                }

                var engagement = new
                {
                    StudentId = studentId,
                    StudentName = student.FullName,
                    LastLoginDate = student.LastLoginAt,
                    TotalTimeSpent = student.Enrollments.Sum(e => e.TimeSpent.TotalHours),
                    ForumParticipation = student.ForumPosts.Count,
                    AssessmentAttempts = student.Enrollments.SelectMany(e => e.AssessmentAttempts).Count(),
                    AverageSessionLength = student.Enrollments.Count > 0 ?
                        student.Enrollments.Average(e => e.TimeSpent.TotalMinutes) : 0,
                    EngagementLevel = CalculateEngagementLevel(student),
                    CoursesInProgress = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                    CompletedCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed)
                };

                return engagement;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating student engagement report for student: {StudentId}", studentId);
                throw;
            }
        }

        private static string CalculateEngagementLevel(User student)
        {
            var totalTimeHours = student.Enrollments.Sum(e => e.TimeSpent.TotalHours);
            var forumPosts = student.ForumPosts.Count;
            var completedCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);

            var score = (totalTimeHours * 0.3) + (forumPosts * 0.2) + (completedCourses * 0.5);

            return score switch
            {
                >= 10 => "High",
                >= 5 => "Medium",
                _ => "Low"
            };
        }

        #endregion

        #region SIS Reports

        public async Task<StudentInformationReportDto> GetStudentInformationReportAsync(User user,string studentId)
        {
            try
            {
                var context = _context;

                var student = await context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .FirstOrDefaultAsync(u => u.Id == studentId);

                if (student == null)
                {
                    throw new ArgumentException($"Student with ID {studentId} not found");
                }

                var totalCoursesEnrolled = student.Enrollments.Count;
                var completedCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);
                var activeCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Active);

                var allGrades = student.Enrollments.Where(e => e.FinalGrade.HasValue).Select(e => e.FinalGrade!.Value).ToList();
                var overallGPA = allGrades.DefaultIfEmpty(0).Average();

                return new StudentInformationReportDto
                {
                    StudentId = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Email = student.Email ?? "",
                    PhoneNumber = student.PhoneNumber ?? "",
                    DateOfBirth = student.DateOfBirth,
                    RegistrationDate = student.CreatedAt,
                    Status = student.IsActive ? "Active" : "Inactive",
                    LastLoginDate = student.LastLoginAt,
                    TotalCoursesEnrolled = totalCoursesEnrolled,
                    CompletedCourses = completedCourses,
                    ActiveCourses = activeCourses,
                    OverallGPA = overallGPA,
                    TotalCredits = completedCourses * 3, // Assuming 3 credits per course
                    EarnedCredits = completedCourses * 3,
                    TotalPoints = student.TotalPoints,
                    Level = student.Level,
                    Bio = student.Bio ?? "",
                    IsActive = student.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating student information report for student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<IEnumerable<StudentInformationReportDto>> GetAllStudentsInformationReportAsync(User user)
        {
            try
            {
                var context = _context;

                var students = await context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .ToListAsync();

                var reports = new List<StudentInformationReportDto>();

                foreach (var student in students)
                {
                    var totalCoursesEnrolled = student.Enrollments.Count;
                    var completedCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);
                    var activeCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Active);

                    var allGrades = student.Enrollments.Where(e => e.FinalGrade.HasValue).Select(e => e.FinalGrade!.Value).ToList();
                    var overallGPA = allGrades.DefaultIfEmpty(0).Average();

                    reports.Add(new StudentInformationReportDto
                    {
                        StudentId = student.Id,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        Email = student.Email ?? "",
                        PhoneNumber = student.PhoneNumber ?? "",
                        DateOfBirth = student.DateOfBirth,
                        RegistrationDate = student.CreatedAt,
                        Status = student.IsActive ? "Active" : "Inactive",
                        LastLoginDate = student.LastLoginAt,
                        TotalCoursesEnrolled = totalCoursesEnrolled,
                        CompletedCourses = completedCourses,
                        ActiveCourses = activeCourses,
                        OverallGPA = overallGPA,
                        TotalCredits = completedCourses * 3,
                        EarnedCredits = completedCourses * 3,
                        TotalPoints = student.TotalPoints,
                        Level = student.Level,
                        Bio = student.Bio ?? "",
                        IsActive = student.IsActive
                    });
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating all students information report");
                throw;
            }
        }

        public async Task<IEnumerable<StudentInformationReportDto>> GetStudentsByStatusReportAsync(User user,string status)
        {
            try
            {
                var context = _context;

                var isActive = status.ToLower() == "active";
                var students = await context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .Where(u => u.IsActive == isActive)
                    .ToListAsync();

                var reports = new List<StudentInformationReportDto>();

                foreach (var student in students)
                {
                    var totalCoursesEnrolled = student.Enrollments.Count;
                    var completedCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);
                    var activeCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Active);

                    var allGrades = student.Enrollments.Where(e => e.FinalGrade.HasValue).Select(e => e.FinalGrade!.Value).ToList();
                    var overallGPA = allGrades.DefaultIfEmpty(0).Average();

                    reports.Add(new StudentInformationReportDto
                    {
                        StudentId = student.Id,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        Email = student.Email ?? "",
                        PhoneNumber = student.PhoneNumber ?? "",
                        DateOfBirth = student.DateOfBirth,
                        RegistrationDate = student.CreatedAt,
                        Status = student.IsActive ? "Active" : "Inactive",
                        LastLoginDate = student.LastLoginAt,
                        TotalCoursesEnrolled = totalCoursesEnrolled,
                        CompletedCourses = completedCourses,
                        ActiveCourses = activeCourses,
                        OverallGPA = overallGPA,
                        TotalCredits = completedCourses * 3,
                        EarnedCredits = completedCourses * 3,
                        TotalPoints = student.TotalPoints,
                        Level = student.Level,
                        Bio = student.Bio ?? "",
                        IsActive = student.IsActive
                    });
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating students by status report for status: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<AttendanceReportDto>> GetAttendanceReportAsync(User user,int classId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // Note: Since there's no Attendance entity in the current model, 
                // this will return placeholder data based on enrollments
                var context = _context;

                var course = await context.Courses
                    .Include(c => c.Enrollments)
                        .ThenInclude(e => e.User)
                    .FirstOrDefaultAsync(c => c.Id == classId);

                if (course == null)
                {
                    return new List<AttendanceReportDto>();
                }

                var reports = new List<AttendanceReportDto>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    foreach (var enrollment in course.Enrollments.Where(e => e.Status == EnrollmentStatus.Active))
                    {
                        // Generate mock attendance data based on engagement
                        var attendanceStatus = GenerateMockAttendance(enrollment, currentDate);

                        reports.Add(new AttendanceReportDto
                        {
                            StudentId = enrollment.UserId,
                            StudentName = enrollment.User.FullName,
                            ClassId = classId,
                            ClassName = course.Title,
                            Date = currentDate,
                            AttendanceStatus = attendanceStatus,
                            CheckInTime = attendanceStatus == "Present" ? TimeSpan.FromHours(9) : null,
                            CheckOutTime = attendanceStatus == "Present" ? TimeSpan.FromHours(10.5) : null,
                            Duration = attendanceStatus == "Present" ? TimeSpan.FromHours(1.5) : null,
                            Notes = "",
                            TotalClasses = (endDate - startDate).Days + 1,
                            PresentCount = reports.Count(r => r.StudentId == enrollment.UserId && r.AttendanceStatus == "Present") + (attendanceStatus == "Present" ? 1 : 0),
                            AbsentCount = reports.Count(r => r.StudentId == enrollment.UserId && r.AttendanceStatus == "Absent") + (attendanceStatus == "Absent" ? 1 : 0),
                            LateCount = reports.Count(r => r.StudentId == enrollment.UserId && r.AttendanceStatus == "Late") + (attendanceStatus == "Late" ? 1 : 0),
                            AttendancePercentage = 0 // Will be calculated after all data is gathered
                        });
                    }
                    currentDate = currentDate.AddDays(1);
                }

                // Calculate attendance percentages
                foreach (var report in reports)
                {
                    var totalForStudent = reports.Count(r => r.StudentId == report.StudentId);
                    var presentForStudent = reports.Count(r => r.StudentId == report.StudentId && r.AttendanceStatus == "Present");
                    report.AttendancePercentage = totalForStudent > 0 ? (double)presentForStudent / totalForStudent * 100 : 0;
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating attendance report for class: {ClassId} from {StartDate} to {EndDate}", classId, startDate, endDate);
                throw;
            }
        }

        private static string GenerateMockAttendance(Enrollment enrollment, DateTime date)
        {
            // Generate mock attendance based on student progress and engagement
            var random = new Random(enrollment.UserId.GetHashCode() + date.DayOfYear);
            var attendanceRate = Math.Min(95, 60 + enrollment.ProgressPercentage * 0.3); // Higher progress = better attendance

            var randomValue = random.Next(100);
            return randomValue < attendanceRate ? "Present" :
                   randomValue < attendanceRate + 5 ? "Late" : "Absent";
        }

        public async Task<IEnumerable<AttendanceReportDto>> GetStudentAttendanceReportAsync(User user,string studentId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var context = _context;

                var enrollments = await context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Course)
                    .Where(e => e.UserId == studentId && e.Status == EnrollmentStatus.Active)
                    .ToListAsync();

                var reports = new List<AttendanceReportDto>();

                foreach (var enrollment in enrollments)
                {
                    var currentDate = startDate;
                    while (currentDate <= endDate)
                    {
                        var attendanceStatus = GenerateMockAttendance(enrollment, currentDate);

                        reports.Add(new AttendanceReportDto
                        {
                            StudentId = enrollment.UserId,
                            StudentName = enrollment.User.FullName,
                            ClassId = enrollment.CourseId,
                            ClassName = enrollment.Course.Title,
                            Date = currentDate,
                            AttendanceStatus = attendanceStatus,
                            CheckInTime = attendanceStatus == "Present" ? TimeSpan.FromHours(9) : null,
                            CheckOutTime = attendanceStatus == "Present" ? TimeSpan.FromHours(10.5) : null,
                            Duration = attendanceStatus == "Present" ? TimeSpan.FromHours(1.5) : null,
                            Notes = ""
                        });

                        currentDate = currentDate.AddDays(1);
                    }
                }

                // Calculate summary statistics
                foreach (var report in reports)
                {
                    var allForClass = reports.Where(r => r.ClassId == report.ClassId).ToList();
                    report.TotalClasses = allForClass.Count;
                    report.PresentCount = allForClass.Count(r => r.AttendanceStatus == "Present");
                    report.AbsentCount = allForClass.Count(r => r.AttendanceStatus == "Absent");
                    report.LateCount = allForClass.Count(r => r.AttendanceStatus == "Late");
                    report.AttendancePercentage = report.TotalClasses > 0 ? (double)report.PresentCount / report.TotalClasses * 100 : 0;
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating student attendance report for student: {StudentId} from {StartDate} to {EndDate}", studentId, startDate, endDate);
                throw;
            }
        }

        public async Task<GradeDistributionReportDto> GetGradeDistributionReportAsync(User user,int classId)
        {
            try
            {
                var context = _context;

                var course = await context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .FirstOrDefaultAsync(c => c.Id == classId);

                if (course == null)
                {
                    throw new ArgumentException($"Course with ID {classId} not found");
                }

                var grades = course.Enrollments
                    .Where(e => e.FinalGrade.HasValue)
                    .Select(e => e.FinalGrade!.Value)
                    .ToList();

                if (!grades.Any())
                {
                    return new GradeDistributionReportDto
                    {
                        ClassId = classId,
                        ClassName = course.Title,
                        InstructorName = course.Instructor?.FullName ?? "Unknown",
                        TotalStudents = course.Enrollments.Count,
                        ReportGeneratedDate = DateTime.UtcNow
                    };
                }

                var aGrades = grades.Count(g => g >= 90);
                var bGrades = grades.Count(g => g >= 80 && g < 90);
                var cGrades = grades.Count(g => g >= 70 && g < 80);
                var dGrades = grades.Count(g => g >= 60 && g < 70);
                var fGrades = grades.Count(g => g < 60);

                var average = grades.Average();
                var median = GetMedian(grades);
                var standardDeviation = GetStandardDeviation(grades, average);

                return new GradeDistributionReportDto
                {
                    ClassId = classId,
                    ClassName = course.Title,
                    InstructorName = course.Instructor?.FullName ?? "Unknown",
                    TotalStudents = course.Enrollments.Count,
                    AGrades = aGrades,
                    BGrades = bGrades,
                    CGrades = cGrades,
                    DGrades = dGrades,
                    FGrades = fGrades,
                    AverageGrade = average,
                    MedianGrade = median,
                    HighestGrade = grades.Max(),
                    LowestGrade = grades.Min(),
                    StandardDeviation = standardDeviation,
                    GradeRange = $"{grades.Min():F1} - {grades.Max():F1}",
                    ReportGeneratedDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating grade distribution report for class: {ClassId}", classId);
                throw;
            }
        }

        private static double GetMedian(List<double> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            var count = sorted.Count;

            if (count == 0) return 0;
            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            else
                return sorted[count / 2];
        }

        private static double GetStandardDeviation(List<double> values, double mean)
        {
            if (values.Count <= 1) return 0;

            var sumSquaredDifferences = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumSquaredDifferences / (values.Count - 1));
        }

        public async Task<IEnumerable<GradeDistributionReportDto>> GetAllClassesGradeDistributionReportAsync(User user)
        {
            try
            {
                var context = _context;

                var courses = await context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .Where(c => c.IsActive)
                    .ToListAsync();

                var reports = new List<GradeDistributionReportDto>();

                foreach (var course in courses)
                {
                    var grades = course.Enrollments
                        .Where(e => e.FinalGrade.HasValue)
                        .Select(e => e.FinalGrade!.Value)
                        .ToList();

                    if (!grades.Any()) continue;

                    var aGrades = grades.Count(g => g >= 90);
                    var bGrades = grades.Count(g => g >= 80 && g < 90);
                    var cGrades = grades.Count(g => g >= 70 && g < 80);
                    var dGrades = grades.Count(g => g >= 60 && g < 70);
                    var fGrades = grades.Count(g => g < 60);

                    var average = grades.Average();
                    var median = GetMedian(grades);
                    var standardDeviation = GetStandardDeviation(grades, average);

                    reports.Add(new GradeDistributionReportDto
                    {
                        ClassId = course.Id,
                        ClassName = course.Title,
                        InstructorName = course.Instructor?.FullName ?? "Unknown",
                        TotalStudents = course.Enrollments.Count,
                        AGrades = aGrades,
                        BGrades = bGrades,
                        CGrades = cGrades,
                        DGrades = dGrades,
                        FGrades = fGrades,
                        AverageGrade = average,
                        MedianGrade = median,
                        HighestGrade = grades.Max(),
                        LowestGrade = grades.Min(),
                        StandardDeviation = standardDeviation,
                        GradeRange = $"{grades.Min():F1} - {grades.Max():F1}",
                        ReportGeneratedDate = DateTime.UtcNow
                    });
                }

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating all classes grade distribution report");
                throw;
            }
        }

        public async Task<TeacherPerformanceReportDto> GetTeacherPerformanceReportAsync(User user,string teacherId)
        {
            try
            {
                var context = _context;

                var teacher = await context.Users
                    .Include(u => u.CreatedCourses)
                        .ThenInclude(c => c.Enrollments)
                    .Include(u => u.ForumPosts)
                    .FirstOrDefaultAsync(u => u.Id == teacherId);

                if (teacher == null)
                {
                    throw new ArgumentException($"Teacher with ID {teacherId} not found");
                }

                var courses = teacher.CreatedCourses.ToList();
                var totalStudents = courses.SelectMany(c => c.Enrollments).Count();
                var allGrades = courses.SelectMany(c => c.Enrollments)
                    .Where(e => e.FinalGrade.HasValue)
                    .Select(e => e.FinalGrade!.Value)
                    .ToList();

                var averageGrade = allGrades.DefaultIfEmpty(0).Average();
                var completedCourses = courses.SelectMany(c => c.Enrollments).Count(e => e.Status == EnrollmentStatus.Completed);
                var totalEnrollments = totalStudents;
                var completionRate = totalEnrollments > 0 ? (double)completedCourses / totalEnrollments * 100 : 0;

                // Mock data for some fields that would require additional tracking
                var forumParticipation = teacher.ForumPosts.Count;
                var messagesReplied = forumParticipation; // Approximation
                var averageGradingTime = 2.5; // Mock: 2.5 days average

                return new TeacherPerformanceReportDto
                {
                    TeacherId = teacher.Id,
                    TeacherName = teacher.FullName,
                    Email = teacher.Email ?? "",
                    TotalCoursesTeaching = courses.Count,
                    TotalStudentsTeaching = totalStudents,
                    AverageClassGrade = averageGrade,
                    StudentSatisfactionRating = GenerateMockSatisfactionRating(averageGrade, completionRate),
                    TotalAssignments = courses.Sum(c => c.Modules.SelectMany(m => m.Lessons).Count()), // Using lessons as assignments
                    GradedAssignments = (int)(courses.Sum(c => c.Modules.SelectMany(m => m.Lessons).Count()) * 0.85), // 85% graded
                    AverageGradingTime = averageGradingTime,
                    ForumParticipation = forumParticipation,
                    MessagesReplied = messagesReplied,
                    LastLoginDate = teacher.LastLoginAt ?? DateTime.UtcNow.AddDays(-7),
                    TotalTimeSpent = TimeSpan.FromHours(totalStudents * 0.5), // Mock calculation
                    CoursesCompleted = completedCourses,
                    CourseCompletionRate = completionRate,
                    PerformanceStatus = GetPerformanceStatus(averageGrade, completionRate, forumParticipation)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating teacher performance report for teacher: {TeacherId}", teacherId);
                throw;
            }
        }

        private static double GenerateMockSatisfactionRating(double averageGrade, double completionRate)
        {
            // Generate satisfaction based on performance metrics
            var baseRating = 3.0;
            var gradeBonus = (averageGrade - 70) / 30.0; // Normalize around 70%
            var completionBonus = (completionRate - 50) / 50.0; // Normalize around 50%

            var rating = baseRating + gradeBonus + completionBonus;
            return Math.Max(1.0, Math.Min(5.0, rating)); // Clamp between 1-5
        }

        private static string GetPerformanceStatus(double averageGrade, double completionRate, int forumParticipation)
        {
            var score = (averageGrade * 0.4) + (completionRate * 0.4) + (Math.Min(forumParticipation, 50) * 0.2);

            return score switch
            {
                >= 80 => "Excellent",
                >= 70 => "Good",
                >= 60 => "Satisfactory",
                _ => "Needs Improvement"
            };
        }

        public async Task<IEnumerable<TeacherPerformanceReportDto>> GetAllTeachersPerformanceReportAsync(User user)
        {
            try
            {
                var context = _context;

                var teachers = await context.Users
                    .Include(u => u.CreatedCourses)
                        .ThenInclude(c => c.Enrollments)
                    .Include(u => u.ForumPosts)
                    .Where(u => u.CreatedCourses.Any()) // Only users who have created courses
                    .ToListAsync();

                var reports = new List<TeacherPerformanceReportDto>();

                foreach (var teacher in teachers)
                {
                    var courses = teacher.CreatedCourses.ToList();
                    var totalStudents = courses.SelectMany(c => c.Enrollments).Count();
                    var allGrades = courses.SelectMany(c => c.Enrollments)
                        .Where(e => e.FinalGrade.HasValue)
                        .Select(e => e.FinalGrade!.Value)
                        .ToList();

                    var averageGrade = allGrades.DefaultIfEmpty(0).Average();
                    var completedCourses = courses.SelectMany(c => c.Enrollments).Count(e => e.Status == EnrollmentStatus.Completed);
                    var completionRate = totalStudents > 0 ? (double)completedCourses / totalStudents * 100 : 0;
                    var forumParticipation = teacher.ForumPosts.Count;

                    reports.Add(new TeacherPerformanceReportDto
                    {
                        TeacherId = teacher.Id,
                        TeacherName = teacher.FullName,
                        Email = teacher.Email ?? "",
                        TotalCoursesTeaching = courses.Count,
                        TotalStudentsTeaching = totalStudents,
                        AverageClassGrade = averageGrade,
                        StudentSatisfactionRating = GenerateMockSatisfactionRating(averageGrade, completionRate),
                        TotalAssignments = courses.Sum(c => c.Modules.SelectMany(m => m.Lessons).Count()),
                        GradedAssignments = (int)(courses.Sum(c => c.Modules.SelectMany(m => m.Lessons).Count()) * 0.85),
                        AverageGradingTime = 2.5,
                        ForumParticipation = forumParticipation,
                        MessagesReplied = forumParticipation,
                        LastLoginDate = teacher.LastLoginAt ?? DateTime.UtcNow.AddDays(-7),
                        TotalTimeSpent = TimeSpan.FromHours(totalStudents * 0.5),
                        CoursesCompleted = completedCourses,
                        CourseCompletionRate = completionRate,
                        PerformanceStatus = GetPerformanceStatus(averageGrade, completionRate, forumParticipation)
                    });
                }

                return reports.OrderByDescending(r => r.StudentSatisfactionRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating all teachers performance report");
                throw;
            }
        }

        #endregion

        #region Advanced Analytics Reports

        public async Task<object> GetAcademicPerformanceTrendsReportAsync(User user,string studentId, int months = 12)
        {
            try
            {
                var context = _context;

                var student = await context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.AssessmentAttempts)
                    .FirstOrDefaultAsync(u => u.Id == studentId);

                if (student == null)
                {
                    return new { Message = "Student not found" };
                }

                var startDate = DateTime.UtcNow.AddMonths(-months);
                var enrollments = student.Enrollments.Where(e => e.EnrolledAt >= startDate).ToList();

                var trends = new
                {
                    StudentId = studentId,
                    StudentName = student.FullName,
                    Period = $"Last {months} months",
                    OverallGPATrend = CalculateGPATrend(enrollments),
                    ProgressTrend = CalculateProgressTrend(enrollments),
                    AssessmentTrends = CalculateAssessmentTrends(enrollments),
                    TimeSpentTrend = enrollments.OrderBy(e => e.EnrolledAt).Select(e => new
                    {
                        Month = e.EnrolledAt.ToString("yyyy-MM"),
                        TimeSpent = e.TimeSpent.TotalHours,
                        Course = e.Course.Title
                    }),
                    CompletionRate = enrollments.Count > 0 ?
                        (double)enrollments.Count(e => e.Status == EnrollmentStatus.Completed) / enrollments.Count * 100 : 0,
                    PredictiveIndicators = new
                    {
                        RiskLevel = CalculateRiskLevel(student),
                        ProjectedGPA = CalculateProjectedGPA(enrollments),
                        RecommendedActions = GetRecommendedActions(student, enrollments)
                    }
                };

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating academic performance trends for student: {StudentId}", studentId);
                throw;
            }
        }

        private static object CalculateGPATrend(List<Enrollment> enrollments)
        {
            var monthlyGPA = enrollments
                .Where(e => e.FinalGrade.HasValue)
                .GroupBy(e => e.CompletedAt?.ToString("yyyy-MM") ?? "In Progress")
                .Select(g => new
                {
                    Month = g.Key,
                    AverageGPA = g.Average(e => e.FinalGrade!.Value)
                })
                .OrderBy(x => x.Month);

            return monthlyGPA;
        }

        private static object CalculateProgressTrend(List<Enrollment> enrollments)
        {
            return enrollments.OrderBy(e => e.EnrolledAt).Select(e => new
            {
                Date = e.EnrolledAt.ToString("yyyy-MM-dd"),
                Progress = e.ProgressPercentage,
                Course = e.Course.Title
            });
        }

        private static object CalculateAssessmentTrends(List<Enrollment> enrollments)
        {
            var assessmentData = enrollments
                .SelectMany(e => e.AssessmentAttempts)
                .Where(aa => aa.Status == AssessmentAttemptStatus.Completed && aa.Score.HasValue)
                .GroupBy(aa => aa.StartedAt.ToString("yyyy-MM"))
                .Select(g => new
                {
                    Month = g.Key,
                    AverageScore = g.Average(aa => aa.Score!.Value),
                    AttemptsCount = g.Count(),
                    PassRate = (double)g.Count(aa => aa.IsPassed) / g.Count() * 100
                })
                .OrderBy(x => x.Month);

            return assessmentData;
        }

        private static string CalculateRiskLevel(User student)
        {
            var activeEnrollments = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Active);
            var averageProgress = student.Enrollments.Where(e => e.Status == EnrollmentStatus.Active).Average(e => e?.ProgressPercentage ?? 0);
            var recentLogin = student.LastLoginAt?.AddDays(30) > DateTime.UtcNow;

            var riskScore = 0;
            if (averageProgress < 30) riskScore += 3;
            else if (averageProgress < 60) riskScore += 2;
            else if (averageProgress < 80) riskScore += 1;

            if (!recentLogin) riskScore += 2;
            if (activeEnrollments > 3) riskScore += 1;

            return riskScore switch
            {
                >= 4 => "High",
                >= 2 => "Medium",
                _ => "Low"
            };
        }

        private static double CalculateProjectedGPA(List<Enrollment> enrollments)
        {
            var completedGrades = enrollments.Where(e => e.FinalGrade.HasValue).Select(e => e.FinalGrade!.Value).ToList();
            var currentProgress = enrollments.Where(e => e.Status == EnrollmentStatus.Active).Average(e => e?.ProgressPercentage ?? 0);

            if (!completedGrades.Any()) return currentProgress * 0.8; // Conservative estimate

            var historicalAverage = completedGrades.Average();
            var progressFactor = currentProgress / 100.0;

            return historicalAverage * 0.7 + (progressFactor * 100) * 0.3;
        }

        private static List<string> GetRecommendedActions(User student, List<Enrollment> enrollments)
        {
            var actions = new List<string>();

            var averageProgress = enrollments.Where(e => e.Status == EnrollmentStatus.Active).Average(e => e?.ProgressPercentage ?? 0);
            if (averageProgress < 50)
            {
                actions.Add("Schedule tutoring sessions");
                actions.Add("Reduce course load");
            }

            if (student.LastLoginAt?.AddDays(7) < DateTime.UtcNow)
            {
                actions.Add("Send engagement reminder");
            }

            var forumParticipation = student.ForumPosts.Count;
            if (forumParticipation < 5)
            {
                actions.Add("Encourage forum participation");
            }

            if (!actions.Any())
            {
                actions.Add("Continue current learning path");
            }

            return actions;
        }

        public async Task<IEnumerable<object>> GetRiskStudentsReportAsync(User user)
        {
            try
            {
                var context = _context;

                var students = await context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .Include(u => u.ForumPosts)
                    .Where(u => u.Enrollments.Any(e => e.Status == EnrollmentStatus.Active))
                    .ToListAsync();

                var riskStudents = new List<object>();

                foreach (var student in students)
                {
                    var riskLevel = CalculateRiskLevel(student);
                    if (riskLevel == "High" || riskLevel == "Medium")
                    {
                        var activeEnrollments = student.Enrollments.Where(e => e.Status == EnrollmentStatus.Active).ToList();
                        var averageProgress = activeEnrollments.Average(e => e.ProgressPercentage);

                        riskStudents.Add(new
                        {
                            StudentId = student.Id,
                            StudentName = student.FullName,
                            Email = student.Email,
                            RiskLevel = riskLevel,
                            AverageProgress = averageProgress,
                            ActiveCourses = activeEnrollments.Count,
                            LastLoginDate = student.LastLoginAt,
                            DaysSinceLastLogin = student.LastLoginAt.HasValue ?
                                (DateTime.UtcNow - student.LastLoginAt.Value).Days : 999,
                            ForumParticipation = student.ForumPosts.Count,
                            RecommendedActions = GetRecommendedActions(student, student.Enrollments.ToList()),
                            Courses = activeEnrollments.Select(e => new
                            {
                                CourseId = e.CourseId,
                                CourseName = e.Course.Title,
                                Progress = e.ProgressPercentage,
                                EnrolledDate = e.EnrolledAt
                            })
                        });
                    }
                }

                return riskStudents.OrderByDescending(r =>
                    r.GetType().GetProperty("RiskLevel")?.GetValue(r)?.ToString() == "High" ? 1 : 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating risk students report");
                throw;
            }
        }

        public async Task<object> GetInstitutionalEffectivenessReportAsync(User user)
        {
            try
            {
                var context = _context;

                var totalStudents = await context.Users.CountAsync();
                var totalCourses = await context.Courses.CountAsync();
                var totalEnrollments = await context.Enrollments.CountAsync();
                var completedEnrollments = await context.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Completed);

                var averageGrade = await context.Enrollments
                    .Where(e => e.FinalGrade.HasValue)
                    .AverageAsync(e => e.FinalGrade!.Value);

                var certificatesIssued = await context.Enrollments.CountAsync(e => e.IsCertificateIssued);
                var activeCourses = await context.Courses.CountAsync(c => c.IsActive);

                var monthlyEnrollments = await context.Enrollments
                    .Where(e => e.EnrolledAt >= DateTime.UtcNow.AddMonths(-12))
                    .GroupBy(e => new { e.EnrolledAt.Year, e.EnrolledAt.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Count = g.Count(),
                        Completions = g.Count(e => e.Status == EnrollmentStatus.Completed)
                    })
                    .ToListAsync();

                var effectiveness = new
                {
                    OverallMetrics = new
                    {
                        TotalStudents = totalStudents,
                        TotalCourses = totalCourses,
                        ActiveCourses = activeCourses,
                        TotalEnrollments = totalEnrollments,
                        OverallCompletionRate = totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments * 100 : 0,
                        AverageGrade = averageGrade,
                        CertificateIssuanceRate = totalEnrollments > 0 ? (double)certificatesIssued / totalEnrollments * 100 : 0
                    },
                    EnrollmentTrends = monthlyEnrollments,
                    StudentSuccess = new
                    {
                        CompletionRate = totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments * 100 : 0,
                        RetentionRate = await CalculateRetentionRate(context),
                        AverageTimeToCompletion = await CalculateAverageTimeToCompletion(context),
                        StudentSatisfaction = 4.2 // Mock data
                    },
                    FacultyMetrics = new
                    {
                        TotalInstructors = await context.Users.CountAsync(u => u.CreatedCourses.Any()),
                        AverageCoursesPerInstructor = await CalculateAverageCoursesPerInstructor(context),
                        StudentToFacultyRatio = await CalculateStudentToFacultyRatio(context)
                    },
                    InstitutionalKPIs = new
                    {
                        StudentEngagementScore = await CalculateEngagementScore(context),
                        TechnologyAdoptionRate = 85.0, // Mock data
                        ResourceUtilizationRate = 78.5, // Mock data
                        QualityScore = CalculateQualityScore(averageGrade, (double)completedEnrollments / totalEnrollments * 100)
                    },
                    GeneratedAt = DateTime.UtcNow
                };

                return effectiveness;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating institutional effectiveness report");
                throw;
            }
        }

        private static async Task<double> CalculateRetentionRate(ApplicationDbContext context)
        {
            var oneYearAgo = DateTime.UtcNow.AddYears(-1);
            var studentsOneYearAgo = await context.Enrollments.CountAsync(e => e.EnrolledAt <= oneYearAgo);
            var stillActiveStudents = await context.Enrollments
                .CountAsync(e => e.EnrolledAt <= oneYearAgo &&
                           (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed));

            return studentsOneYearAgo > 0 ? (double)stillActiveStudents / studentsOneYearAgo * 100 : 0;
        }

        private static async Task<double> CalculateAverageTimeToCompletion(ApplicationDbContext context)
        {
            var completedEnrollments = await context.Enrollments
                .Where(e => e.Status == EnrollmentStatus.Completed && e.StartedAt.HasValue && e.CompletedAt.HasValue)
                .ToListAsync();

            if (!completedEnrollments.Any()) return 0;

            return completedEnrollments.Average(e => (e.CompletedAt!.Value - e.StartedAt!.Value).TotalDays);
        }

        private static async Task<double> CalculateAverageCoursesPerInstructor(ApplicationDbContext context)
        {
            var instructors = await context.Users.Where(u => u.CreatedCourses.Any()).CountAsync();
            var totalCourses = await context.Courses.CountAsync();

            return instructors > 0 ? (double)totalCourses / instructors : 0;
        }

        private static async Task<double> CalculateStudentToFacultyRatio(ApplicationDbContext context)
        {
            var totalStudents = await context.Users.CountAsync();
            var faculty = await context.Users.CountAsync(u => u.CreatedCourses.Any());

            return faculty > 0 ? (double)totalStudents / faculty : 0;
        }

        private static async Task<double> CalculateEngagementScore(ApplicationDbContext context)
        {
            var totalUsers = await context.Users.CountAsync();
            var activeUsers = await context.Users.CountAsync(u => u.LastLoginAt >= DateTime.UtcNow.AddDays(-30));
            var forumPosts = await context.ForumPosts.CountAsync();

            var loginRate = totalUsers > 0 ? (double)activeUsers / totalUsers : 0;
            var postRate = totalUsers > 0 ? (double)forumPosts / totalUsers : 0;

            return (loginRate * 70) + (Math.Min(postRate * 10, 30)); // Weighted score out of 100
        }

        private static double CalculateQualityScore(double averageGrade, double completionRate)
        {
            return (averageGrade * 0.6) + (completionRate * 0.4);
        }

        public async Task<object> GetResourceUtilizationReportAsync(User user)
        {
            try
            {
                var context = _context;

                var totalCourses = await context.Courses.CountAsync();
                var activeCourses = await context.Courses.CountAsync(c => c.IsActive);
                var totalModules = await context.Modules.CountAsync();
                var totalLessons = await context.Lessons.CountAsync();
                var totalAssessments = await context.Assessments.CountAsync();
                var totalForums = await context.Forums.CountAsync();

                var enrollments = await context.Enrollments.Include(e => e.ModuleProgresses).ToListAsync();
                var moduleProgresses = enrollments.SelectMany(e => e.ModuleProgresses).ToList();

                var utilization = new
                {
                    CourseUtilization = new
                    {
                        TotalCourses = totalCourses,
                        ActiveCourses = activeCourses,
                        UtilizationRate = totalCourses > 0 ? (double)activeCourses / totalCourses * 100 : 0,
                        AverageEnrollmentsPerCourse = totalCourses > 0 ? (double)enrollments.Count / totalCourses : 0
                    },
                    ContentUtilization = new
                    {
                        TotalModules = totalModules,
                        CompletedModules = moduleProgresses.Count(mp => mp.IsCompleted),
                        ModuleCompletionRate = totalModules > 0 ? (double)moduleProgresses.Count(mp => mp.IsCompleted) / totalModules * 100 : 0,
                        TotalLessons = totalLessons,
                        LessonUtilizationRate = 75.5, // Mock calculation
                        AverageTimePerModule = moduleProgresses.Where(mp => mp.IsCompleted).Average(mp => mp?.CompletedAt != null && mp.StartedAt != null ?
                            (mp.CompletedAt.Value - mp.StartedAt.Value).TotalHours : 2.5)
                    },
                    AssessmentUtilization = new
                    {
                        TotalAssessments = totalAssessments,
                        AssessmentsWithAttempts = await context.AssessmentAttempts.Select(aa => aa.AssessmentId).Distinct().CountAsync(),
                        UtilizationRate = totalAssessments > 0 ?
                            (double)await context.AssessmentAttempts.Select(aa => aa.AssessmentId).Distinct().CountAsync() / totalAssessments * 100 : 0,
                        AverageAttemptsPerAssessment = totalAssessments > 0 ?
                            (double)await context.AssessmentAttempts.CountAsync() / totalAssessments : 0
                    },
                    ForumUtilization = new
                    {
                        TotalForums = totalForums,
                        ActiveForums = await context.Forums.CountAsync(f => f.Topics.Any()),
                        UtilizationRate = totalForums > 0 ?
                            (double)await context.Forums.CountAsync(f => f.Topics.Any()) / totalForums * 100 : 0,
                        TotalPosts = await context.ForumPosts.CountAsync(),
                        AveragePostsPerForum = totalForums > 0 ?
                            (double)await context.ForumPosts.CountAsync() / totalForums : 0
                    },
                    SystemMetrics = new
                    {
                        TotalUsers = await context.Users.CountAsync(),
                        ActiveUsers = await context.Users.CountAsync(u => u.LastLoginAt >= DateTime.UtcNow.AddDays(-30)),
                        UserEngagementRate = await context.Users.CountAsync() > 0 ?
                            (double)await context.Users.CountAsync(u => u.LastLoginAt >= DateTime.UtcNow.AddDays(-30)) /
                            await context.Users.CountAsync() * 100 : 0,
                        StorageUtilization = new
                        {
                            TotalFiles = 1250, // Mock data
                            TotalSizeGB = 45.7, // Mock data
                            AverageFileSize = 37.3 // Mock data (MB)
                        }
                    },
                    Recommendations = GenerateResourceRecommendations(activeCourses, totalCourses, totalAssessments, await context.AssessmentAttempts.CountAsync()),
                    GeneratedAt = DateTime.UtcNow
                };

                return utilization;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating resource utilization report");
                throw;
            }
        }

        private static List<string> GenerateResourceRecommendations(int activeCourses, int totalCourses, int totalAssessments, int totalAttempts)
        {
            var recommendations = new List<string>();

            var courseUtilization = totalCourses > 0 ? (double)activeCourses / totalCourses * 100 : 0;
            if (courseUtilization < 70)
            {
                recommendations.Add("Consider archiving underutilized courses");
                recommendations.Add("Review course catalog for relevance");
            }

            var assessmentUtilization = totalAssessments > 0 ? (double)totalAttempts / totalAssessments : 0;
            if (assessmentUtilization < 5)
            {
                recommendations.Add("Increase assessment engagement through gamification");
            }

            if (recommendations.Count == 0)
            {
                recommendations.Add("Resource utilization is optimal");
            }

            return recommendations;
        }

        public async Task<object> GetRetentionAnalysisReportAsync(User user)
        {
            try
            {
                var context = _context;

                var enrollments = await context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Course)
                    .ToListAsync();

                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                var oneYearAgo = DateTime.UtcNow.AddYears(-1);

                var cohorts = enrollments
                    .GroupBy(e => new { Year = e.EnrolledAt.Year, Month = e.EnrolledAt.Month })
                    .Select(g => new
                    {
                        Cohort = $"{g.Key.Year}-{g.Key.Month:D2}",
                        TotalEnrolled = g.Count(),
                        StillActive = g.Count(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed),
                        Completed = g.Count(e => e.Status == EnrollmentStatus.Completed),
                        Dropped = g.Count(e => e.Status == EnrollmentStatus.Dropped),
                        RetentionRate = g.Count() > 0 ? (double)g.Count(e => e.Status != EnrollmentStatus.Dropped) / g.Count() * 100 : 0,
                        CompletionRate = g.Count() > 0 ? (double)g.Count(e => e.Status == EnrollmentStatus.Completed) / g.Count() * 100 : 0
                    })
                    .OrderByDescending(c => c.Cohort)
                    .Take(24); // Last 24 months

                var dropoutReasons = await AnalyzeDropoutReasons(context);
                var retentionFactors = await AnalyzeRetentionFactors(context);

                var analysis = new
                {
                    OverallMetrics = new
                    {
                        TotalEnrollments = enrollments.Count,
                        CurrentlyActive = enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                        TotalCompleted = enrollments.Count(e => e.Status == EnrollmentStatus.Completed),
                        TotalDropped = enrollments.Count(e => e.Status == EnrollmentStatus.Dropped),
                        OverallRetentionRate = enrollments.Count > 0 ?
                            (double)enrollments.Count(e => e.Status != EnrollmentStatus.Dropped) / enrollments.Count * 100 : 0,
                        OverallCompletionRate = enrollments.Count > 0 ?
                            (double)enrollments.Count(e => e.Status == EnrollmentStatus.Completed) / enrollments.Count * 100 : 0
                    },
                    CohortAnalysis = cohorts,
                    DropoutAnalysis = dropoutReasons,
                    RetentionFactors = retentionFactors,
                    Trends = new
                    {
                        SixMonthRetention = CalculatePeriodRetention(enrollments, sixMonthsAgo),
                        OneYearRetention = CalculatePeriodRetention(enrollments, oneYearAgo),
                        AverageTimeToCompletion = enrollments
                            .Where(e => e.Status == EnrollmentStatus.Completed && e.StartedAt.HasValue && e.CompletedAt.HasValue)
                            .Average(e => (e.CompletedAt!.Value - e.StartedAt!.Value).TotalDays),
                        AverageTimeToDropout = enrollments
                            .Where(e => e.Status == EnrollmentStatus.Dropped && e.StartedAt.HasValue)
                            .Average(e => (DateTime.UtcNow - e.StartedAt!.Value).TotalDays)
                    },
                    InterventionRecommendations = GenerateInterventionRecommendations(enrollments),
                    GeneratedAt = DateTime.UtcNow
                };

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating retention analysis report");
                throw;
            }
        }

        private static async Task<object> AnalyzeDropoutReasons(ApplicationDbContext context)
        {
            var droppedEnrollments = await context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Where(e => e.Status == EnrollmentStatus.Dropped)
                .ToListAsync();

            return new
            {
                TotalDropouts = droppedEnrollments.Count,
                EarlyDropouts = droppedEnrollments.Count(e => e.ProgressPercentage < 25),
                MidCourseDropouts = droppedEnrollments.Count(e => e.ProgressPercentage >= 25 && e.ProgressPercentage < 75),
                LateDropouts = droppedEnrollments.Count(e => e.ProgressPercentage >= 75),
                CommonPatterns = new
                {
                    LowEngagement = droppedEnrollments.Count(e => e.TimeSpent.TotalHours < 5),
                    NoAssessmentAttempts = droppedEnrollments.Count(e => !e.AssessmentAttempts.Any()),
                    QuickDropout = droppedEnrollments.Count(e => e.StartedAt.HasValue &&
                        (DateTime.UtcNow - e.StartedAt.Value).TotalDays < 7)
                }
            };
        }

        private static async Task<object> AnalyzeRetentionFactors(ApplicationDbContext context)
        {
            var completedEnrollments = await context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Include(e => e.AssessmentAttempts)
                .Where(e => e.Status == EnrollmentStatus.Completed)
                .ToListAsync();

            return new
            {
                SuccessFactors = new
                {
                    HighEngagement = completedEnrollments.Count(e => e.TimeSpent.TotalHours > 20),
                    RegularAssessments = completedEnrollments.Count(e => e.AssessmentAttempts.Count >= 3),
                    GoodGrades = completedEnrollments.Count(e => e.FinalGrade >= 80),
                    QuickStart = completedEnrollments.Count(e => e.StartedAt.HasValue &&
                        (e.StartedAt.Value - e.EnrolledAt).TotalDays <= 3)
                },
                OptimalDuration = completedEnrollments
                    .Where(e => e.StartedAt.HasValue && e.CompletedAt.HasValue)
                    .Average(e => (e.CompletedAt!.Value - e.StartedAt!.Value).TotalDays)
            };
        }

        private static double CalculatePeriodRetention(List<Enrollment> enrollments, DateTime cutoffDate)
        {
            var periodEnrollments = enrollments.Where(e => e.EnrolledAt >= cutoffDate).ToList();
            if (!periodEnrollments.Any()) return 0;

            return (double)periodEnrollments.Count(e => e.Status != EnrollmentStatus.Dropped) / periodEnrollments.Count * 100;
        }

        private static List<string> GenerateInterventionRecommendations(List<Enrollment> enrollments)
        {
            var recommendations = new List<string>();

            var earlyDropoutRate = enrollments.Count(e => e.Status == EnrollmentStatus.Dropped && e.ProgressPercentage < 25);
            var totalDropouts = enrollments.Count(e => e.Status == EnrollmentStatus.Dropped);

            if (totalDropouts > 0 && (double)earlyDropoutRate / totalDropouts > 0.5)
            {
                recommendations.Add("Implement early engagement interventions");
                recommendations.Add("Improve onboarding process");
            }

            var lowEngagementCount = enrollments.Count(e => e.Status == EnrollmentStatus.Active && e.ProgressPercentage < 10);
            if (lowEngagementCount > enrollments.Count * 0.1)
            {
                recommendations.Add("Send re-engagement campaigns to low-progress students");
            }

            if (recommendations.Count == 0)
            {
                recommendations.Add("Current retention strategies are effective");
            }

            return recommendations;
        }

        #endregion

        #region Export Methods

        public async Task<byte[]> ExportReportToPdfAsync(User user, string reportType, object parameters)
        {
            try
            {
                // Basic PDF export implementation
                // Note: This is a simplified implementation. In production, you would use libraries like iTextSharp, PdfSharp, or similar

                var reportData = await GetReportDataByType(user,reportType, parameters);
                var pdfContent = GeneratePdfContent(reportType, reportData);

                // Convert to bytes (this is a mock implementation)
                var pdfBytes = System.Text.Encoding.UTF8.GetBytes(pdfContent);

                // In a real implementation, you would:
                // 1. Use a PDF library to create actual PDF content
                // 2. Add proper formatting, tables, charts
                // 3. Include company branding and styling

                await Task.Delay(1); // Simulate processing time
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report to PDF: {ReportType}", reportType);
                throw;
            }
        }

        public async Task<byte[]> ExportReportToExcelAsync(User user,string reportType, object parameters)
        {
            try
            {
                // Basic Excel export implementation
                // Note: This is a simplified implementation. In production, you would use libraries like EPPlus, ClosedXML, or similar

                var reportData = await GetReportDataByType(user,reportType, parameters);
                var excelContent = GenerateExcelContent(reportType, reportData);

                // Convert to bytes (this is a mock implementation)
                var excelBytes = System.Text.Encoding.UTF8.GetBytes(excelContent);

                // In a real implementation, you would:
                // 1. Use an Excel library to create actual Excel files
                // 2. Add multiple worksheets, charts, pivot tables
                // 3. Apply formatting and styling
                // 4. Include formulas and calculations

                await Task.Delay(1); // Simulate processing time
                return excelBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report to Excel: {ReportType}", reportType);
                throw;
            }
        }

        public async Task<string> ExportReportToCsvAsync(User user,string reportType, object parameters)
        {
            try
            {
                var reportData = await GetReportDataByType(user,reportType, parameters);
                var csvContent = GenerateCsvContent(reportType, reportData);

                // In a real implementation, you might save this to a file and return the file path
                // For now, we'll return the CSV content directly

                await Task.Delay(1); // Simulate processing time
                return csvContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report to CSV: {ReportType}", reportType);
                throw;
            }
        }

        private async Task<object> GetReportDataByType(User user,string reportType, object parameters)
        {
            // Route to appropriate report method based on type
            return reportType.ToLower() switch
            {
                "studentprogress" => await GetStudentProgressReportAsync(user),
                "coursecompletion" => await GetAllCoursesCompletionReportAsync(user),
                "assessmentperformance" => await GetDifficultAssessmentsReportAsync(user),
                "enrollment" => await GetEnrollmentSummaryReportAsync(user,DateTime.UtcNow.AddMonths(-12), DateTime.UtcNow),
                "forum" => await GetAllForumsActivityReportAsync(user),
                "students" => await GetAllStudentsInformationReportAsync(user),
                "teachers" => await GetAllTeachersPerformanceReportAsync(user),
                "gradedistribution" => await GetAllClassesGradeDistributionReportAsync(user),
                _ => new { Message = "Report type not supported", Type = reportType }
            };
        }

        private static string GeneratePdfContent(string reportType, object reportData)
        {
            // Mock PDF content generation
            return $@"
                PDF Report: {reportType}
                Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
                
                Report Data:
                {System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}
                
                [Note: This is a mock PDF implementation. In production, use proper PDF libraries.]
            ";
        }

        private static string GenerateExcelContent(string reportType, object reportData)
        {
            // Mock Excel content generation (CSV format for simplicity)
            var header = reportType switch
            {
                "studentprogress" => "Student ID,Student Name,Course,Progress %,Status",
                "coursecompletion" => "Course ID,Course Name,Total Enrollments,Completion Rate,Status",
                _ => "Data,Value,Type,Generated"
            };

            return $@"
                {header}
                [Mock Excel Data]
                Generated,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss},Report,{reportType}
                
                [Note: This is a mock Excel implementation. In production, use proper Excel libraries.]
            ";
        }

        private static string GenerateCsvContent(string reportType, object reportData)
        {
            // Basic CSV generation
            var header = reportType.ToLower() switch
            {
                "studentprogress" => "StudentId,StudentName,CourseName,ProgressPercentage,Status,EnrolledAt",
                "coursecompletion" => "CourseId,CourseName,TotalEnrollments,CompletionRate,InstructorName,CourseStatus",
                "enrollment" => "Date,TotalEnrollments,NewEnrollments,CompletedEnrollments,Period",
                _ => "Type,Value,GeneratedAt"
            };

            var csvBuilder = new System.Text.StringBuilder();
            csvBuilder.AppendLine(header);

            // Add a sample row (in production, iterate through actual data)
            csvBuilder.AppendLine($"Sample,Data,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            csvBuilder.AppendLine($"ReportType,{reportType},{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

            return csvBuilder.ToString();
        }

        #endregion
    }
}
