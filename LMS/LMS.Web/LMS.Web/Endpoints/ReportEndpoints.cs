// LMS.Web/LMS.Web/Endpoints/ReportEndpoints.cs
using LMS.Data.Entities;
using LMS.Repositories;
using LMS.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Web.Endpoints;

public class ReportEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reports")
            .WithTags("Reports")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Instructor" });

        // LMS Reports
        group.MapGet("student-progress", GetStudentProgress);
        group.MapGet("student-progress-by-courses/{studentId}", GetStudentProgressByCourses);
        group.MapGet("course-completion/{courseId:int}", GetCourseCompletion);
        group.MapGet("all-courses-completion", GetAllCoursesCompletion);
        group.MapGet("assessment-performance/{assessmentId:int}", GetAssessmentPerformance);
        group.MapGet("course-assessments-performance/{courseId:int}", GetCourseAssessmentsPerformance);
        group.MapGet("enrollment-summary", GetEnrollmentSummary);
        group.MapGet("enrollment-trends", GetEnrollmentTrends);
        group.MapGet("forum-activity/{forumId:int}", GetForumActivity);
        group.MapGet("all-forums-activity", GetAllForumsActivity);

        // Advanced LMS Reports
        group.MapGet("low-performance-students", GetLowPerformanceStudents);
        group.MapGet("popular-courses", GetPopularCourses);
        group.MapGet("difficult-assessments", GetDifficultAssessments);
        group.MapGet("learning-analytics/{courseId:int}", GetLearningAnalytics);
        group.MapGet("student-engagement/{studentId}", GetStudentEngagement);

        // SIS Reports
        group.MapGet("student-info/{studentId}", GetStudentInformation);
        group.MapGet("all-students-info", GetAllStudentsInformation);
        group.MapGet("students-by-status/{status}", GetStudentsByStatus);
        group.MapGet("attendance/{classId:int}", GetAttendance);
        group.MapGet("student-attendance/{studentId}", GetStudentAttendance);
        group.MapGet("grade-distribution/{classId:int}", GetGradeDistribution);
        group.MapGet("all-classes-grade-distribution", GetAllClassesGradeDistribution);
        group.MapGet("teacher-performance/{teacherId}", GetTeacherPerformance);
        group.MapGet("all-teachers-performance", GetAllTeachersPerformance);

        // Advanced SIS Reports
        group.MapGet("academic-performance-trends/{studentId}", GetAcademicPerformanceTrends);
        group.MapGet("risk-students", GetRiskStudents);
        group.MapGet("institutional-effectiveness", GetInstitutionalEffectiveness);
        group.MapGet("resource-utilization", GetResourceUtilization);
        group.MapGet("retention-analysis", GetRetentionAnalysis);

        // Export Reports
        group.MapPost("export/pdf", ExportReportToPdf);
        group.MapPost("export/excel", ExportReportToExcel);
        group.MapPost("export/csv", ExportReportToCsv);
    }

    // LMS Reports
    private static async Task<IResult> GetStudentProgress(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, [FromQuery] string? studentId = null)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetStudentProgressReportAsync(user, studentId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetStudentProgressByCourses(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, string studentId)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetStudentProgressByCoursesAsync(user, studentId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetCourseCompletion(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, int courseId)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetCourseCompletionReportAsync(user, courseId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetAllCoursesCompletion(
        HttpContext context, UserManager<User> userManager, IReportRepository repo)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetAllCoursesCompletionReportAsync(user);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetAssessmentPerformance(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, int assessmentId)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetAssessmentPerformanceReportAsync(user, assessmentId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetCourseAssessmentsPerformance(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, int courseId)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetCourseAssessmentsPerformanceReportAsync(user, courseId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetEnrollmentSummary(
        HttpContext context, UserManager<User> userManager, IReportRepository repo,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string period = "Daily")
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;
            var report = await repo.GetEnrollmentSummaryReportAsync(user, start, end, period);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetEnrollmentTrends(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, [FromQuery] int months = 12)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetEnrollmentTrendsReportAsync(user, months);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetForumActivity(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, int forumId)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetForumActivityReportAsync(user, forumId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetAllForumsActivity(
        HttpContext context, UserManager<User> userManager, IReportRepository repo)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetAllForumsActivityReportAsync(user);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    // Advanced LMS Reports
    private static async Task<IResult> GetLowPerformanceStudents(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, [FromQuery] double threshold = 50.0)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetLowPerformanceStudentsReportAsync(user, threshold);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetPopularCourses(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, [FromQuery] int topN = 10)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetPopularCoursesReportAsync(user, topN);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetDifficultAssessments(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, [FromQuery] double passRateThreshold = 70.0)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetDifficultAssessmentsReportAsync(user, passRateThreshold);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetLearningAnalytics(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, int courseId)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetLearningAnalyticsReportAsync(user, courseId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetStudentEngagement(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, string studentId)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetStudentEngagementReportAsync(user, studentId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    // SIS Reports
    private static async Task<IResult> GetStudentInformation(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, string studentId)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetStudentInformationReportAsync(user, studentId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetAllStudentsInformation(
        HttpContext context, UserManager<User> userManager, IReportRepository repo)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetAllStudentsInformationReportAsync(user);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetStudentsByStatus(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, string status)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetStudentsByStatusReportAsync(user, status);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetAttendance(
        HttpContext context, UserManager<User> userManager, IReportRepository repo,
        int classId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;
            var report = await repo.GetAttendanceReportAsync(user, classId, start, end);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetStudentAttendance(
        HttpContext context, UserManager<User> userManager, IReportRepository repo,
        string studentId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;
            var report = await repo.GetStudentAttendanceReportAsync(user, studentId, start, end);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetGradeDistribution(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, int classId)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetGradeDistributionReportAsync(user, classId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetAllClassesGradeDistribution(
        HttpContext context, UserManager<User> userManager, IReportRepository repo)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetAllClassesGradeDistributionReportAsync(user);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetTeacherPerformance(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, string teacherId)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetTeacherPerformanceReportAsync(user, teacherId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetAllTeachersPerformance(
        HttpContext context, UserManager<User> userManager, IReportRepository repo)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetAllTeachersPerformanceReportAsync(user);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    // Advanced SIS Reports
    private static async Task<IResult> GetAcademicPerformanceTrends(
        HttpContext context, UserManager<User> userManager, IReportRepository repo,
        string studentId, [FromQuery] int months = 12)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetAcademicPerformanceTrendsReportAsync(user, studentId, months);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetRiskStudents(
        HttpContext context, UserManager<User> userManager, IReportRepository repo)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetRiskStudentsReportAsync(user);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetInstitutionalEffectiveness(
        HttpContext context, UserManager<User> userManager, IReportRepository repo)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetInstitutionalEffectivenessReportAsync(user);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetResourceUtilization(
        HttpContext context, UserManager<User> userManager, IReportRepository repo)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetResourceUtilizationReportAsync(user);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetRetentionAnalysis(
        HttpContext context, UserManager<User> userManager, IReportRepository repo)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var report = await repo.GetRetentionAnalysisReportAsync(user);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    // Export Reports
    private static async Task<IResult> ExportReportToPdf(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, [FromBody] ExportRequest request)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var result = await repo.ExportReportToPdfAsync(user, request.ReportType, request.Parameters);
            return Results.File(result, "application/pdf", $"{request.ReportType}.pdf");
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> ExportReportToExcel(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, [FromBody] ExportRequest request)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var result = await repo.ExportReportToExcelAsync(user, request.ReportType, request.Parameters);
            return Results.File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{request.ReportType}.xlsx");
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> ExportReportToCsv(
        HttpContext context, UserManager<User> userManager, IReportRepository repo, [FromBody] ExportRequest request)
    {
        try
        {
            var user = await userManager.GetUserAsync(context.User);
            var result = await repo.ExportReportToCsvAsync(user, request.ReportType, request.Parameters);
            return Results.Text(result, "text/csv");
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    public class ExportRequest
    {
        public string ReportType { get; set; } = string.Empty;
        public object Parameters { get; set; } = new();
    }
}