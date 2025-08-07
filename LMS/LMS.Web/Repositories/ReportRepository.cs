using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LMS.Web.Repositories.DTOs;
using LMS.Web.Data;
using Microsoft.EntityFrameworkCore;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace LMS.Repositories
{
    public interface IReportRepository
    {
        // LMS Reports
        Task<IEnumerable<StudentProgressReportDto>> GetStudentProgressReportAsync(User user, string? studentId = null, DateTime? startDate = null, DateTime? endDate = null, int? courseId = null, string? status = null);
        Task<IEnumerable<StudentProgressReportDto>> GetStudentProgressByCoursesAsync(User user, string studentId, DateTime? startDate = null, DateTime? endDate = null);
        Task<CourseCompletionReportDto> GetCourseCompletionReportAsync(User user, int courseId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<CourseCompletionReportDto>> GetAllCoursesCompletionReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, string? instructorId = null, string? category = null);
        Task<AssessmentPerformanceReportDto> GetAssessmentPerformanceReportAsync(User user, int assessmentId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<AssessmentPerformanceReportDto>> GetCourseAssessmentsPerformanceReportAsync(User user, int courseId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<EnrollmentSummaryReportDto>> GetEnrollmentSummaryReportAsync(User user, DateTime startDate, DateTime endDate, string period = "Daily", int? courseId = null, string? instructorId = null);
        Task<IEnumerable<EnrollmentSummaryReportDto>> GetEnrollmentTrendsReportAsync(User user, int months = 12, int? courseId = null, string? instructorId = null);
        Task<ForumActivityReportDto> GetForumActivityReportAsync(User user, int forumId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<ForumActivityReportDto>> GetAllForumsActivityReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, int? courseId = null);

        // Advanced LMS Reports
        Task<IEnumerable<StudentProgressReportDto>> GetLowPerformanceStudentsReportAsync(User user, double threshold = 50.0, DateTime? startDate = null, DateTime? endDate = null, int? courseId = null);
        Task<IEnumerable<CourseCompletionReportDto>> GetPopularCoursesReportAsync(User user, int topN = 10, DateTime? startDate = null, DateTime? endDate = null, string? category = null);
        Task<IEnumerable<AssessmentPerformanceReportDto>> GetDifficultAssessmentsReportAsync(User user, double passRateThreshold = 70.0, DateTime? startDate = null, DateTime? endDate = null, int? courseId = null);
        Task<object> GetLearningAnalyticsReportAsync(User user, int courseId, DateTime? startDate = null, DateTime? endDate = null);
        Task<object> GetStudentEngagementReportAsync(User user, string studentId, DateTime? startDate = null, DateTime? endDate = null);

        // SIS Reports
        Task<StudentInformationReportDto> GetStudentInformationReportAsync(User user, string studentId);
        Task<IEnumerable<StudentInformationReportDto>> GetAllStudentsInformationReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, string? status = null, int? courseId = null);
        Task<IEnumerable<StudentInformationReportDto>> GetStudentsByStatusReportAsync(User user, string status, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<AttendanceReportDto>> GetAttendanceReportAsync(User user, int classId, DateTime startDate, DateTime endDate, string? studentId = null);
        Task<IEnumerable<AttendanceReportDto>> GetStudentAttendanceReportAsync(User user, string studentId, DateTime startDate, DateTime endDate, int? classId = null);
        Task<GradeDistributionReportDto> GetGradeDistributionReportAsync(User user, int classId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<GradeDistributionReportDto>> GetAllClassesGradeDistributionReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, string? instructorId = null);
        Task<TeacherPerformanceReportDto> GetTeacherPerformanceReportAsync(User user, string teacherId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<TeacherPerformanceReportDto>> GetAllTeachersPerformanceReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null);

        // Advanced SIS Reports
        Task<object> GetAcademicPerformanceTrendsReportAsync(User user, string studentId, int months = 12, DateTime? endDate = null);
        Task<IEnumerable<object>> GetRiskStudentsReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, double threshold = 2.0);
        Task<object> GetInstitutionalEffectivenessReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null);
        Task<object> GetResourceUtilizationReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null);
        Task<object> GetRetentionAnalysisReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, int cohortMonths = 12);

        // Export Reports
        Task<byte[]> ExportReportToPdfAsync(User user, string reportType, object parameters);
        Task<byte[]> ExportReportToExcelAsync(User user, string reportType, object parameters);
        Task<string> ExportReportToCsvAsync(User user, string reportType, object parameters);
    }

    public class ReportRepository : IReportRepository
    {
        private readonly ILogger<ReportRepository> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ReportRepository(ILogger<ReportRepository> logger, ApplicationDbContext context, UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        #region LMS Reports

        #region Helper Methods

        private async Task<bool> IsInstructorAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.Contains("Instructor") || roles.Contains("Teacher");
        }

        private async Task<List<int>> GetInstructorCourseIdsAsync(User user)
        {
            return await _context.Courses
                .Where(c => c.InstructorId == user.Id)
                .Select(c => c.Id)
                .ToListAsync();
        }

        private IQueryable<Enrollment> ApplyEnrollmentFilters(IQueryable<Enrollment> query, User user, string? studentId = null, DateTime? startDate = null, DateTime? endDate = null, int? courseId = null, string? status = null, List<int>? instructorCourseIds = null)
        {
            // Apply instructor filtering if user is instructor
            if (instructorCourseIds != null && instructorCourseIds.Any())
            {
                query = query.Where(e => instructorCourseIds.Contains(e.CourseId));
            }

            // Apply student filter
            if (!string.IsNullOrEmpty(studentId))
            {
                query = query.Where(e => e.UserId == studentId);
            }

            // Apply date filters
            if (startDate.HasValue)
            {
                query = query.Where(e => e.EnrolledAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.EnrolledAt <= endDate.Value);
            }

            // Apply course filter
            if (courseId.HasValue)
            {
                query = query.Where(e => e.CourseId == courseId.Value);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<EnrollmentStatus>(status, true, out var enrollmentStatus))
            {
                query = query.Where(e => e.Status == enrollmentStatus);
            }

            return query;
        }

        private IQueryable<Course> ApplyCourseFilters(IQueryable<Course> query, User user, DateTime? startDate = null, DateTime? endDate = null, string? instructorId = null, string? category = null, List<int>? instructorCourseIds = null)
        {
            // Apply instructor filtering if user is instructor
            if (instructorCourseIds != null && instructorCourseIds.Any())
            {
                query = query.Where(c => instructorCourseIds.Contains(c.Id));
            }

            // Apply date filters
            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value || c.EndDate == null);
            }

            // Apply instructor filter
            if (!string.IsNullOrEmpty(instructorId))
            {
                query = query.Where(c => c.InstructorId == instructorId);
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(c => c.CourseCategories.Any(cc => cc.Category.Name.Contains(category)));
            }

            return query;
        }

        #endregion

        public async Task<IEnumerable<StudentProgressReportDto>> GetStudentProgressReportAsync(User user, string? studentId = null, DateTime? startDate = null, DateTime? endDate = null, int? courseId = null, string? status = null)
        {
            try
            {
                // Check user role and get instructor course restrictions
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var enrollmentsQuery = _context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Course)
                    .Include(e => e.AssessmentAttempts)
                    .Include(e => e.ModuleProgresses)
                        .ThenInclude(mp => mp.Module)
                            .ThenInclude(m => m.Lessons)
                    .AsQueryable();

                // Apply filters including role-based restrictions
                enrollmentsQuery = ApplyEnrollmentFilters(enrollmentsQuery, user, studentId, startDate, endDate, courseId, status, instructorCourseIds);

                var enrollments = await enrollmentsQuery.ToListAsync();
                var reports = new List<StudentProgressReportDto>();

                foreach (var enrollment in enrollments)
                {
                    var totalModules = await _context.Modules.CountAsync();
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

        public async Task<IEnumerable<StudentProgressReportDto>> GetStudentProgressByCoursesAsync(User user, string studentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Check user role and get instructor course restrictions
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var enrollmentsQuery = _context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Course)
                    .Include(e => e.AssessmentAttempts)
                    .Include(e => e.ModuleProgresses)
                        .ThenInclude(mp => mp.Module)
                            .ThenInclude(m => m.Lessons)
                    .Where(e => e.UserId == studentId)
                    .AsQueryable();

                // Apply date filters and instructor restrictions
                if (instructorCourseIds != null && instructorCourseIds.Any())
                {
                    enrollmentsQuery = enrollmentsQuery.Where(e => instructorCourseIds.Contains(e.CourseId));
                }

                if (startDate.HasValue)
                {
                    enrollmentsQuery = enrollmentsQuery.Where(e => e.EnrolledAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    enrollmentsQuery = enrollmentsQuery.Where(e => e.EnrolledAt <= endDate.Value);
                }

                var enrollments = await enrollmentsQuery.ToListAsync();

                var reports = new List<StudentProgressReportDto>();

                foreach (var enrollment in enrollments)
                {
                    var totalModules = await _context.Modules.CountAsync();
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

        public async Task<CourseCompletionReportDto> GetCourseCompletionReportAsync(User user, int courseId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Check user role and course access
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                // Check if instructor has access to this course
                if (instructorCourseIds != null && !instructorCourseIds.Contains(courseId))
                {
                    throw new UnauthorizedAccessException("You don't have access to this course's data");
                }

                var courseQuery = _context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .Where(c => c.Id == courseId);

                var course = await courseQuery.FirstOrDefaultAsync();

                if (course == null)
                {
                    throw new ArgumentException($"Course with ID {courseId} not found");
                }

                // Apply date filters to enrollments
                var enrollments = course.Enrollments.AsQueryable();
                if (startDate.HasValue)
                {
                    enrollments = enrollments.Where(e => e.EnrolledAt >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    enrollments = enrollments.Where(e => e.EnrolledAt <= endDate.Value);
                }

                var filteredEnrollments = enrollments.ToList();
                var totalEnrollments = filteredEnrollments.Count;
                var completedEnrollments = filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Completed);
                var activeEnrollments = filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Active);
                var droppedEnrollments = filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Dropped);

                var averageCompletionTime = filteredEnrollments
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
                    AverageFinalGrade = filteredEnrollments.Where(e => e.FinalGrade.HasValue).Select(e => e.FinalGrade!.Value).DefaultIfEmpty(0).Average(),
                    CertificatesIssued = filteredEnrollments.Count(e => e.IsCertificateIssued),
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

        public async Task<IEnumerable<CourseCompletionReportDto>> GetAllCoursesCompletionReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, string? instructorId = null, string? category = null)
        {
            try
            {
                // Check user role and get instructor course restrictions
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var coursesQuery = _context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .Include(c => c.CourseCategories)
                        .ThenInclude(cc => cc.Category)
                    .Where(c => c.IsActive)
                    .AsQueryable();

                // Apply filters including role-based restrictions
                coursesQuery = ApplyCourseFilters(coursesQuery, user, startDate, endDate, instructorId, category, instructorCourseIds);

                var courses = await coursesQuery.ToListAsync();

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

        public async Task<AssessmentPerformanceReportDto> GetAssessmentPerformanceReportAsync(User user, int assessmentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Check user role and assessment access
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var assessment = await _context.Assessments
                    .Include(a => a.Course)
                    .Include(a => a.Attempts)
                    .FirstOrDefaultAsync(a => a.Id == assessmentId);

                if (assessment == null)
                {
                    throw new ArgumentException($"Assessment with ID {assessmentId} not found");
                }

                // Check if instructor has access to this assessment's course
                if (instructorCourseIds != null && assessment.CourseId.HasValue && !instructorCourseIds.Contains(assessment.CourseId.Value))
                {
                    throw new UnauthorizedAccessException("You don't have access to this assessment's data");
                }

                // Apply date filters to attempts
                var attempts = assessment.Attempts.Where(a => a.Status == AssessmentAttemptStatus.Completed).AsQueryable();

                if (startDate.HasValue)
                {
                    attempts = attempts.Where(a => a.CompletedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    attempts = attempts.Where(a => a.CompletedAt <= endDate.Value);
                }

                var filteredAttempts = attempts.ToList();
                var totalAttempts = filteredAttempts.Count;
                var passedAttempts = filteredAttempts.Count(a => a.IsPassed);
                var failedAttempts = totalAttempts - passedAttempts;
                var scores = filteredAttempts.Where(a => a.Score.HasValue).Select(a => a.Score!.Value).ToList();

                var uniqueStudents = filteredAttempts.Select(a => a.Enrollment.UserId).Distinct().Count();
                var completionTimes = filteredAttempts.Where(a => a.TimeTaken.HasValue).Select(a => a.TimeTaken!.Value.TotalMinutes).ToList();

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
                    LastAttemptDate = filteredAttempts.OrderByDescending(a => a.CompletedAt).FirstOrDefault()?.CompletedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating assessment performance report for assessment: {AssessmentId}", assessmentId);
                throw;
            }
        }

        public async Task<IEnumerable<AssessmentPerformanceReportDto>> GetCourseAssessmentsPerformanceReportAsync(User user, int courseId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Check user role and course access
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                // Check if instructor has access to this course
                if (instructorCourseIds != null && !instructorCourseIds.Contains(courseId))
                {
                    throw new UnauthorizedAccessException("You don't have access to this course's data");
                }

                var assessments = await _context.Assessments
                    .Include(a => a.Course)
                    .Include(a => a.Attempts)
                    .Where(a => a.CourseId == courseId)
                    .ToListAsync();

                var reports = new List<AssessmentPerformanceReportDto>();

                foreach (var assessment in assessments)
                {
                    // Apply date filters to attempts
                    var attempts = assessment.Attempts.Where(a => a.Status == AssessmentAttemptStatus.Completed).AsQueryable();

                    if (startDate.HasValue)
                    {
                        attempts = attempts.Where(a => a.CompletedAt >= startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        attempts = attempts.Where(a => a.CompletedAt <= endDate.Value);
                    }

                    var filteredAttempts = attempts.ToList();
                    var totalAttempts = filteredAttempts.Count;
                    var passedAttempts = filteredAttempts.Count(a => a.IsPassed);
                    var scores = filteredAttempts.Where(a => a.Score.HasValue).Select(a => a.Score!.Value).ToList();
                    var uniqueStudents = filteredAttempts.Select(a => a.Enrollment.UserId).Distinct().Count();
                    var completionTimes = filteredAttempts.Where(a => a.TimeTaken.HasValue).Select(a => a.TimeTaken!.Value.TotalMinutes).ToList();

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
                        LastAttemptDate = filteredAttempts.OrderByDescending(a => a.CompletedAt).FirstOrDefault()?.CompletedAt
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

        public async Task<IEnumerable<EnrollmentSummaryReportDto>> GetEnrollmentSummaryReportAsync(User user, DateTime startDate, DateTime endDate, string period = "Daily", int? courseId = null, string? instructorId = null)
        {
            try
            {
                // Check user role and get instructor course restrictions
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var enrollmentsQuery = _context.Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.EnrolledAt >= startDate && e.EnrolledAt <= endDate)
                    .AsQueryable();

                // Apply role-based filtering
                if (instructorCourseIds != null && instructorCourseIds.Any())
                {
                    enrollmentsQuery = enrollmentsQuery.Where(e => instructorCourseIds.Contains(e.CourseId));
                }

                // Apply additional filters
                if (courseId.HasValue)
                {
                    enrollmentsQuery = enrollmentsQuery.Where(e => e.CourseId == courseId.Value);
                }

                if (!string.IsNullOrEmpty(instructorId))
                {
                    enrollmentsQuery = enrollmentsQuery.Where(e => e.Course.InstructorId == instructorId);
                }

                var enrollments = await enrollmentsQuery.ToListAsync();

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

        public async Task<IEnumerable<EnrollmentSummaryReportDto>> GetEnrollmentTrendsReportAsync(User user, int months = 12, int? courseId = null, string? instructorId = null)
        {
            try
            {
                // Get enrollment trends over specified number of months
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddMonths(-months);

                return await GetEnrollmentSummaryReportAsync(user, startDate, endDate, "Monthly", courseId, instructorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating enrollment trends report for {Months} months", months);
                throw;
            }
        }

        public async Task<ForumActivityReportDto> GetForumActivityReportAsync(User user, int forumId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Check user role and forum access
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

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

                // Check if instructor has access to this forum's course
                if (instructorCourseIds != null && forum.Course != null && !instructorCourseIds.Contains(forum.Course.Id))
                {
                    throw new UnauthorizedAccessException("You don't have access to this forum's data");
                }

                // Apply date filters to posts
                var allPosts = forum.Topics.SelectMany(t => t.Posts).AsQueryable();

                if (startDate.HasValue)
                {
                    allPosts = allPosts.Where(p => p.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    allPosts = allPosts.Where(p => p.CreatedAt <= endDate.Value);
                }

                var filteredPosts = allPosts.ToList();
                var totalTopics = forum.Topics.Count;
                var totalPosts = filteredPosts.Count;
                var totalViews = forum.Topics.Count; // Since ViewCount is not available, use topic count as approximation
                var activeUsers = filteredPosts.Select(p => p.AuthorId).Distinct().Count();

                var lastActivity = filteredPosts.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue;

                var mostActiveUser = filteredPosts
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

        public async Task<IEnumerable<ForumActivityReportDto>> GetAllForumsActivityReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, int? courseId = null)
        {
            try
            {
                // Check user role and get instructor course restrictions
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var forumsQuery = _context.Forums
                    .Include(f => f.Course)
                    .Include(f => f.Topics)
                        .ThenInclude(t => t.Posts)
                            .ThenInclude(p => p.Author)
                    .Where(f => f.IsActive)
                    .AsQueryable();

                // Apply role-based filtering
                if (instructorCourseIds != null && instructorCourseIds.Any())
                {
                    forumsQuery = forumsQuery.Where(f => f.Course != null && instructorCourseIds.Contains(f.Course.Id));
                }

                // Apply course filter
                if (courseId.HasValue)
                {
                    forumsQuery = forumsQuery.Where(f => f.Course != null && f.Course.Id == courseId.Value);
                }

                var forums = await forumsQuery.ToListAsync();

                var reports = new List<ForumActivityReportDto>();

                foreach (var forum in forums)
                {
                    // Apply date filters to posts
                    var allPosts = forum.Topics.SelectMany(t => t.Posts).AsQueryable();

                    if (startDate.HasValue)
                    {
                        allPosts = allPosts.Where(p => p.CreatedAt >= startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        allPosts = allPosts.Where(p => p.CreatedAt <= endDate.Value);
                    }

                    var filteredPosts = allPosts.ToList();
                    var totalTopics = forum.Topics.Count;
                    var totalPosts = filteredPosts.Count;
                    var totalViews = forum.Topics.Count; // Approximation
                    var activeUsers = filteredPosts.Select(p => p.AuthorId).Distinct().Count();

                    var lastActivity = filteredPosts.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue;

                    var mostActiveUser = filteredPosts
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

        public async Task<IEnumerable<StudentProgressReportDto>> GetLowPerformanceStudentsReportAsync(User user, double threshold = 50.0, DateTime? startDate = null, DateTime? endDate = null, int? courseId = null)
        {
            try
            {
                // Check user role and get instructor course restrictions
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var enrollmentsQuery = _context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Course)
                    .Include(e => e.AssessmentAttempts)
                    .Where(e => e.ProgressPercentage < threshold ||
                               (e.FinalGrade.HasValue && e.FinalGrade.Value < threshold))
                    .AsQueryable();

                // Apply filters including role-based restrictions
                enrollmentsQuery = ApplyEnrollmentFilters(enrollmentsQuery, user, null, startDate, endDate, courseId, null, instructorCourseIds);

                var enrollments = await enrollmentsQuery.ToListAsync();

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

        public async Task<IEnumerable<CourseCompletionReportDto>> GetPopularCoursesReportAsync(User user, int topN = 10, DateTime? startDate = null, DateTime? endDate = null, string? category = null)
        {
            try
            {
                // Check user role and get instructor course restrictions
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var coursesQuery = _context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .Include(c => c.CourseCategories)
                        .ThenInclude(cc => cc.Category)
                    .Where(c => c.IsActive)
                    .AsQueryable();

                // Apply filters including role-based restrictions
                coursesQuery = ApplyCourseFilters(coursesQuery, user, startDate, endDate, null, category, instructorCourseIds);

                var courses = await coursesQuery
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

        public async Task<IEnumerable<AssessmentPerformanceReportDto>> GetDifficultAssessmentsReportAsync(User user, double passRateThreshold = 70.0, DateTime? startDate = null, DateTime? endDate = null, int? courseId = null)
        {
            try
            {
                // Check user role and get instructor course restrictions
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var assessmentsQuery = _context.Assessments
                    .Include(a => a.Course)
                    .Include(a => a.Attempts)
                    .AsQueryable();

                // Apply role-based filtering
                if (instructorCourseIds != null && instructorCourseIds.Any())
                {
                    assessmentsQuery = assessmentsQuery.Where(a => a.CourseId.HasValue && instructorCourseIds.Contains(a.CourseId.Value));
                }

                // Apply course filter
                if (courseId.HasValue)
                {
                    assessmentsQuery = assessmentsQuery.Where(a => a.CourseId == courseId.Value);
                }

                var assessments = await assessmentsQuery.ToListAsync();

                var reports = new List<AssessmentPerformanceReportDto>();

                foreach (var assessment in assessments)
                {
                    // Apply date filters to attempts
                    var attempts = assessment.Attempts.Where(a => a.Status == AssessmentAttemptStatus.Completed).AsQueryable();

                    if (startDate.HasValue)
                    {
                        attempts = attempts.Where(a => a.CompletedAt >= startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        attempts = attempts.Where(a => a.CompletedAt <= endDate.Value);
                    }

                    var filteredAttempts = attempts.ToList();
                    if (filteredAttempts.Count == 0) continue;

                    var passedAttempts = filteredAttempts.Count(a => a.IsPassed);
                    var passRate = (double)passedAttempts / filteredAttempts.Count * 100;

                    if (passRate < passRateThreshold)
                    {
                        var scores = filteredAttempts.Where(a => a.Score.HasValue).Select(a => a.Score!.Value).ToList();
                        var uniqueStudents = filteredAttempts.Select(a => a.Enrollment.UserId).Distinct().Count();
                        var completionTimes = filteredAttempts.Where(a => a.TimeTaken.HasValue).Select(a => a.TimeTaken!.Value.TotalMinutes).ToList();

                        reports.Add(new AssessmentPerformanceReportDto
                        {
                            AssessmentId = assessment.Id,
                            AssessmentTitle = assessment.Title,
                            AssessmentType = assessment.Type.ToString(),
                            CourseName = assessment.Course?.Title ?? "N/A",
                            TotalAttempts = filteredAttempts.Count,
                            PassedAttempts = passedAttempts,
                            FailedAttempts = filteredAttempts.Count - passedAttempts,
                            PassRate = passRate,
                            AverageScore = scores.DefaultIfEmpty(0).Average(),
                            HighestScore = scores.DefaultIfEmpty(0).Max(),
                            LowestScore = scores.DefaultIfEmpty(0).Min(),
                            AverageCompletionTime = completionTimes.DefaultIfEmpty(0).Average(),
                            PassingScore = assessment.PassingScore,
                            UniqueStudents = uniqueStudents,
                            LastAttemptDate = filteredAttempts.OrderByDescending(a => a.CompletedAt).FirstOrDefault()?.CompletedAt
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

        public async Task<object> GetLearningAnalyticsReportAsync(User user, int courseId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Check user role and course access
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                // Check if instructor has access to this course
                if (instructorCourseIds != null && !instructorCourseIds.Contains(courseId))
                {
                    throw new UnauthorizedAccessException("You don't have access to this course's data");
                }

                var courseQuery = _context.Courses
                    .Include(c => c.Enrollments)
                        .ThenInclude(e => e.ModuleProgresses)
                    .Include(c => c.Modules)
                        .ThenInclude(m => m.Lessons)
                    .Where(c => c.Id == courseId);

                var course = await courseQuery.FirstOrDefaultAsync();

                if (course == null)
                {
                    return new { Message = "Course not found" };
                }

                // Apply date filters to enrollments
                var enrollments = course.Enrollments.AsQueryable();
                if (startDate.HasValue)
                {
                    enrollments = enrollments.Where(e => e.EnrolledAt >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    enrollments = enrollments.Where(e => e.EnrolledAt <= endDate.Value);
                }

                var filteredEnrollments = enrollments.ToList();

                var analytics = new
                {
                    CourseId = courseId,
                    CourseName = course.Title,
                    TotalStudents = filteredEnrollments.Count,
                    AverageProgress = filteredEnrollments.Count > 0 ? filteredEnrollments.Average(e => e.ProgressPercentage) : 0,
                    CompletionRate = filteredEnrollments.Count > 0 ?
                        (double)filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Completed) / filteredEnrollments.Count * 100 : 0,
                    PopularModules = course.Modules.Select(m => new
                    {
                        ModuleId = m.Id,
                        ModuleName = m.Title,
                        CompletionRate = filteredEnrollments.Count > 0 ?
                            filteredEnrollments.Count(e => e.ModuleProgresses.Any(mp => mp.ModuleId == m.Id && mp.IsCompleted)) / (double)filteredEnrollments.Count * 100 : 0
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

        public async Task<object> GetStudentEngagementReportAsync(User user, string studentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Check user role and student access
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var studentQuery = _context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .Include(u => u.ForumPosts)
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.AssessmentAttempts)
                    .Where(u => u.Id == studentId);

                var student = await studentQuery.FirstOrDefaultAsync();

                if (student == null)
                {
                    return new { Message = "Student not found" };
                }

                // Apply role-based filtering to enrollments
                var enrollments = student.Enrollments.AsQueryable();
                if (instructorCourseIds != null && instructorCourseIds.Any())
                {
                    enrollments = enrollments.Where(e => instructorCourseIds.Contains(e.CourseId));
                }

                // Apply date filters
                if (startDate.HasValue)
                {
                    enrollments = enrollments.Where(e => e.EnrolledAt >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    enrollments = enrollments.Where(e => e.EnrolledAt <= endDate.Value);
                }

                var filteredEnrollments = enrollments.ToList();

                // Apply date filters to forum posts
                var forumPosts = student.ForumPosts.AsQueryable();
                if (startDate.HasValue)
                {
                    forumPosts = forumPosts.Where(p => p.CreatedAt >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    forumPosts = forumPosts.Where(p => p.CreatedAt <= endDate.Value);
                }

                var filteredForumPosts = forumPosts.ToList();

                var engagement = new
                {
                    StudentId = studentId,
                    StudentName = student.FullName,
                    LastLoginDate = student.LastLoginAt,
                    TotalTimeSpent = filteredEnrollments.Sum(e => e.TimeSpent.TotalHours),
                    ForumParticipation = filteredForumPosts.Count,
                    AssessmentAttempts = filteredEnrollments.SelectMany(e => e.AssessmentAttempts).Count(),
                    AverageSessionLength = filteredEnrollments.Count > 0 ?
                        filteredEnrollments.Average(e => e.TimeSpent.TotalMinutes) : 0,
                    EngagementLevel = CalculateFilteredEngagementLevel(filteredEnrollments, filteredForumPosts),
                    CoursesInProgress = filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Active),
                    CompletedCourses = filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Completed)
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

        private static string CalculateFilteredEngagementLevel(List<Enrollment> enrollments, List<ForumPost> forumPosts)
        {
            var totalTimeHours = enrollments.Sum(e => e.TimeSpent.TotalHours);
            var forumPostCount = forumPosts.Count;
            var completedCourses = enrollments.Count(e => e.Status == EnrollmentStatus.Completed);

            var score = (totalTimeHours * 0.3) + (forumPostCount * 0.2) + (completedCourses * 0.5);

            return score switch
            {
                >= 10 => "High",
                >= 5 => "Medium",
                _ => "Low"
            };
        }

        #endregion

        #region SIS Reports

        public async Task<StudentInformationReportDto> GetStudentInformationReportAsync(User user, string studentId)
        {
            try
            {
                // Check user role and student access
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var context = _context;

                var student = await context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .FirstOrDefaultAsync(u => u.Id == studentId);

                if (student == null)
                {
                    throw new ArgumentException($"Student with ID {studentId} not found");
                }

                // Apply role-based filtering to enrollments
                var enrollments = student.Enrollments.AsQueryable();
                if (instructorCourseIds != null && instructorCourseIds.Any())
                {
                    enrollments = enrollments.Where(e => instructorCourseIds.Contains(e.CourseId));
                }

                var filteredEnrollments = enrollments.ToList();

                var totalCoursesEnrolled = filteredEnrollments.Count;
                var completedCourses = filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Completed);
                var activeCourses = filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Active);

                var allGrades = filteredEnrollments.Where(e => e.FinalGrade.HasValue).Select(e => e.FinalGrade!.Value).ToList();
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

        public async Task<IEnumerable<StudentInformationReportDto>> GetAllStudentsInformationReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, string? status = null, int? courseId = null)
        {
            try
            {
                // Check user role and get instructor course restrictions
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var context = _context;

                var studentsQuery = context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .AsQueryable();

                // Apply status filter
                if (!string.IsNullOrEmpty(status))
                {
                    var isActive = status.ToLower() == "active";
                    studentsQuery = studentsQuery.Where(u => u.IsActive == isActive);
                }

                // Apply date filter on registration
                if (startDate.HasValue)
                {
                    studentsQuery = studentsQuery.Where(u => u.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    studentsQuery = studentsQuery.Where(u => u.CreatedAt <= endDate.Value);
                }

                // Apply course filter (students enrolled in specific course)
                if (courseId.HasValue)
                {
                    studentsQuery = studentsQuery.Where(u => u.Enrollments.Any(e => e.CourseId == courseId.Value));
                }

                var students = await studentsQuery.ToListAsync();

                var reports = new List<StudentInformationReportDto>();

                foreach (var student in students)
                {
                    // Apply role-based filtering to enrollments
                    var enrollments = student.Enrollments.AsQueryable();
                    if (instructorCourseIds != null && instructorCourseIds.Any())
                    {
                        enrollments = enrollments.Where(e => instructorCourseIds.Contains(e.CourseId));
                    }

                    var filteredEnrollments = enrollments.ToList();

                    var totalCoursesEnrolled = filteredEnrollments.Count;
                    var completedCourses = filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Completed);
                    var activeCourses = filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Active);

                    var allGrades = filteredEnrollments.Where(e => e.FinalGrade.HasValue).Select(e => e.FinalGrade!.Value).ToList();
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

        public async Task<IEnumerable<StudentInformationReportDto>> GetStudentsByStatusReportAsync(User user, string status, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Check user role and get instructor course restrictions
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                var context = _context;

                var isActive = status.ToLower() == "active";
                var studentsQuery = context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .Where(u => u.IsActive == isActive)
                    .AsQueryable();

                // Apply date filters on registration
                if (startDate.HasValue)
                {
                    studentsQuery = studentsQuery.Where(u => u.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    studentsQuery = studentsQuery.Where(u => u.CreatedAt <= endDate.Value);
                }

                var students = await studentsQuery.ToListAsync();

                var reports = new List<StudentInformationReportDto>();

                foreach (var student in students)
                {
                    // Apply role-based filtering to enrollments
                    var enrollments = student.Enrollments.AsQueryable();
                    if (instructorCourseIds != null && instructorCourseIds.Any())
                    {
                        enrollments = enrollments.Where(e => instructorCourseIds.Contains(e.CourseId));
                    }

                    var filteredEnrollments = enrollments.ToList();

                    var totalCoursesEnrolled = filteredEnrollments.Count;
                    var completedCourses = filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Completed);
                    var activeCourses = filteredEnrollments.Count(e => e.Status == EnrollmentStatus.Active);

                    var allGrades = filteredEnrollments.Where(e => e.FinalGrade.HasValue).Select(e => e.FinalGrade!.Value).ToList();
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

        public async Task<IEnumerable<AttendanceReportDto>> GetAttendanceReportAsync(User user, int classId, DateTime startDate, DateTime endDate, string? studentId = null)
        {
            try
            {
                // Check user role and course access
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                // Check if instructor has access to this course
                if (instructorCourseIds != null && !instructorCourseIds.Contains(classId))
                {
                    throw new UnauthorizedAccessException("You don't have access to this course's data");
                }

                // Query actual attendance records from the Attendance entity
                var attendanceQuery = _context.Attendances
                    .Include(a => a.Student)
                    .Include(a => a.Class)
                    .Where(a => a.ClassId == classId && a.Date >= startDate && a.Date <= endDate)
                    .AsQueryable();

                // Apply student filter if provided
                if (!string.IsNullOrEmpty(studentId))
                {
                    attendanceQuery = attendanceQuery.Where(a => a.StudentId == studentId);
                }

                var attendanceRecords = await attendanceQuery.ToListAsync();

                var reports = attendanceRecords.Select(a => new AttendanceReportDto
                {
                    StudentId = a.StudentId ?? string.Empty,
                    StudentName = a.Student != null ? $"{a.Student.FirstName} {a.Student.LastName}" : "Unknown Student",
                    ClassId = a.ClassId,
                    ClassName = a.Class?.Title ?? "Unknown Course",
                    Date = a.Date,
                    AttendanceStatus = a.Status.ToString(),
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    Duration = a.Duration,
                    Notes = a.Notes ?? string.Empty
                }).ToList();

                // Calculate summary statistics for each student
                foreach (var studentGroup in reports.GroupBy(r => r.StudentId))
                {
                    var studentReports = studentGroup.ToList();
                    var totalClasses = studentReports.Count;
                    var presentCount = studentReports.Count(r => r.AttendanceStatus == "Present");
                    var absentCount = studentReports.Count(r => r.AttendanceStatus == "Absent");
                    var lateCount = studentReports.Count(r => r.AttendanceStatus == "Late");
                    var attendancePercentage = totalClasses > 0 ? (double)(presentCount + lateCount) / totalClasses * 100 : 0;

                    foreach (var report in studentReports)
                    {
                        report.TotalClasses = totalClasses;
                        report.PresentCount = presentCount;
                        report.AbsentCount = absentCount;
                        report.LateCount = lateCount;
                        report.AttendancePercentage = Math.Round(attendancePercentage, 2);
                    }
                }

                return reports.OrderBy(r => r.StudentName).ThenBy(r => r.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating attendance report for class: {ClassId} from {StartDate} to {EndDate}", classId, startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<AttendanceReportDto>> GetStudentAttendanceReportAsync(User user, string studentId, DateTime startDate, DateTime endDate, int? classId = null)
        {
            try
            {
                // Check user role and student access
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                // Query actual attendance records from the Attendance entity
                var attendanceQuery = _context.Attendances
                    .Include(a => a.Student)
                    .Include(a => a.Class)
                    .Where(a => a.StudentId == studentId && a.Date >= startDate && a.Date <= endDate)
                    .AsQueryable();

                // Apply role-based filtering for instructors
                if (instructorCourseIds != null && instructorCourseIds.Any())
                {
                    attendanceQuery = attendanceQuery.Where(a => instructorCourseIds.Contains(a.ClassId));
                }

                // Apply class filter if provided
                if (classId.HasValue)
                {
                    attendanceQuery = attendanceQuery.Where(a => a.ClassId == classId.Value);
                }

                var attendanceRecords = await attendanceQuery.ToListAsync();

                var reports = attendanceRecords.Select(a => new AttendanceReportDto
                {
                    StudentId = a.StudentId ?? string.Empty,
                    StudentName = a.Student != null ? $"{a.Student.FirstName} {a.Student.LastName}" : "Unknown Student",
                    ClassId = a.ClassId,
                    ClassName = a.Class?.Title ?? "Unknown Course",
                    Date = a.Date,
                    AttendanceStatus = a.Status.ToString(),
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    Duration = a.Duration,
                    Notes = a.Notes ?? string.Empty
                }).ToList();

                // Calculate summary statistics for each class
                foreach (var classGroup in reports.GroupBy(r => r.ClassId))
                {
                    var classReports = classGroup.ToList();
                    var totalClasses = classReports.Count;
                    var presentCount = classReports.Count(r => r.AttendanceStatus == "Present");
                    var absentCount = classReports.Count(r => r.AttendanceStatus == "Absent");
                    var lateCount = classReports.Count(r => r.AttendanceStatus == "Late");
                    var attendancePercentage = totalClasses > 0 ? (double)(presentCount + lateCount) / totalClasses * 100 : 0;

                    foreach (var report in classReports)
                    {
                        report.TotalClasses = totalClasses;
                        report.PresentCount = presentCount;
                        report.AbsentCount = absentCount;
                        report.LateCount = lateCount;
                        report.AttendancePercentage = Math.Round(attendancePercentage, 2);
                    }
                }

                return reports.OrderBy(r => r.ClassName).ThenBy(r => r.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating student attendance report for student: {StudentId} from {StartDate} to {EndDate}", studentId, startDate, endDate);
                throw;
            }
        }

        public async Task<GradeDistributionReportDto> GetGradeDistributionReportAsync(User user, int classId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Check user role and course access
                var isInstructor = await IsInstructorAsync(user);
                var instructorCourseIds = isInstructor ? await GetInstructorCourseIdsAsync(user) : null;

                // Check if instructor has access to this course
                if (instructorCourseIds != null && !instructorCourseIds.Contains(classId))
                {
                    throw new UnauthorizedAccessException("You don't have access to this course's data");
                }

                var context = _context;

                var course = await context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .FirstOrDefaultAsync(c => c.Id == classId);

                if (course == null)
                {
                    throw new ArgumentException($"Course with ID {classId} not found");
                }

                // Apply date filters to enrollments
                var enrollments = course.Enrollments.AsQueryable();
                if (startDate.HasValue)
                {
                    enrollments = enrollments.Where(e => e.EnrolledAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    enrollments = enrollments.Where(e => e.EnrolledAt <= endDate.Value);
                }

                var filteredEnrollments = enrollments.ToList();
                var grades = filteredEnrollments
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
                        TotalStudents = filteredEnrollments.Count,
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
                    TotalStudents = filteredEnrollments.Count,
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

        // Missing interface implementations
        public async Task<TeacherPerformanceReportDto> GetTeacherPerformanceReportAsync(User user, string teacherId, DateTime? startDate = null, DateTime? endDate = null)
        {
            // TODO: Implement actual logic. For now, return a dummy object for build success.
            await Task.CompletedTask;
            return new TeacherPerformanceReportDto();
        }
        public async Task<object> GetAcademicPerformanceTrendsReportAsync(User user, string studentId, int months = 12, DateTime? endDate = null)
        {
            await Task.CompletedTask;
            return new { };
        }
        public async Task<object> GetInstitutionalEffectivenessReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null)
        {
            await Task.CompletedTask;
            return new { };
        }
        public async Task<object> GetRetentionAnalysisReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, int cohortMonths = 12)
        {
            await Task.CompletedTask;
            return new { };
        }
        public async Task<byte[]> ExportReportToPdfAsync(User user, string reportType, object parameters)
        {
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }
        public async Task<byte[]> ExportReportToExcelAsync(User user, string reportType, object parameters)
        {
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }
        public async Task<string> ExportReportToCsvAsync(User user, string reportType, object parameters)
        {
            await Task.CompletedTask;
            return string.Empty;
        }
        // --- BEGIN: Add missing interface method stubs for build success ---
        public async Task<IEnumerable<GradeDistributionReportDto>> GetAllClassesGradeDistributionReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, string? instructorId = null)
        {
            await Task.CompletedTask;
            return new List<GradeDistributionReportDto>();
        }
        public async Task<IEnumerable<TeacherPerformanceReportDto>> GetAllTeachersPerformanceReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null)
        {
            await Task.CompletedTask;
            return new List<TeacherPerformanceReportDto>();
        }
        public async Task<IEnumerable<object>> GetRiskStudentsReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null, double threshold = 2.0)
        {
            await Task.CompletedTask;
            return new List<object>();
        }
        public async Task<object> GetResourceUtilizationReportAsync(User user, DateTime? startDate = null, DateTime? endDate = null)
        {
            await Task.CompletedTask;
            return new { };
        }
        // --- END: Add missing interface method stubs for build success ---
        #endregion
    }
}
