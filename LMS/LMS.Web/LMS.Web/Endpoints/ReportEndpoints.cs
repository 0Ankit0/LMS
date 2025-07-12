using LMS.Web.Infrastructure;
using LMS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Web.Endpoints;

public class ReportEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reports")
            .WithTags("Reports");

        // LMS Reports
        group.MapGet("student-progress", GetStudentProgress)
            .WithName("GetStudentProgress")
            .WithSummary("Get student progress report");

        group.MapGet("course-completion/{courseId:int}", GetCourseCompletion)
            .WithName("GetCourseCompletion")
            .WithSummary("Get course completion report");

        group.MapGet("assessment-performance/{assessmentId:int}", GetAssessmentPerformance)
            .WithName("GetAssessmentPerformance")
            .WithSummary("Get assessment performance report");

        group.MapGet("enrollment-summary", GetEnrollmentSummary)
            .WithName("GetEnrollmentSummary")
            .WithSummary("Get enrollment summary report");

        group.MapGet("forum-activity/{forumId:int}", GetForumActivity)
            .WithName("GetForumActivity")
            .WithSummary("Get forum activity report");

        // SIS Reports
        group.MapGet("student-info/{studentId}", GetStudentInformation)
            .WithName("GetStudentInformation")
            .WithSummary("Get student information report");

        group.MapGet("attendance/{classId:int}", GetAttendance)
            .WithName("GetAttendance")
            .WithSummary("Get attendance report");

        group.MapGet("grade-distribution/{classId:int}", GetGradeDistribution)
            .WithName("GetGradeDistribution")
            .WithSummary("Get grade distribution report");

        group.MapGet("teacher-performance/{teacherId}", GetTeacherPerformance)
            .WithName("GetTeacherPerformance")
            .WithSummary("Get teacher performance report");
    }

    private static async Task<IResult> GetStudentProgress(
        IReportRepository reportRepository,
        [FromQuery] string? studentId = null)
    {
        try
        {
            var report = await reportRepository.GetStudentProgressReportAsync(studentId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetCourseCompletion(
        IReportRepository reportRepository,
        int courseId)
    {
        try
        {
            var report = await reportRepository.GetCourseCompletionReportAsync(courseId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetAssessmentPerformance(
        IReportRepository reportRepository,
        int assessmentId)
    {
        try
        {
            var report = await reportRepository.GetAssessmentPerformanceReportAsync(assessmentId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetEnrollmentSummary(
        IReportRepository reportRepository,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string period = "Daily")
    {
        try
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;
            var report = await reportRepository.GetEnrollmentSummaryReportAsync(start, end, period);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetForumActivity(
        IReportRepository reportRepository,
        int forumId)
    {
        try
        {
            var report = await reportRepository.GetForumActivityReportAsync(forumId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetStudentInformation(
        IReportRepository reportRepository,
        string studentId)
    {
        try
        {
            var report = await reportRepository.GetStudentInformationReportAsync(studentId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetAttendance(
        IReportRepository reportRepository,
        int classId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;
            var report = await reportRepository.GetAttendanceReportAsync(classId, start, end);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetGradeDistribution(
        IReportRepository reportRepository,
        int classId)
    {
        try
        {
            var report = await reportRepository.GetGradeDistributionReportAsync(classId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetTeacherPerformance(
        IReportRepository reportRepository,
        string teacherId)
    {
        try
        {
            var report = await reportRepository.GetTeacherPerformanceReportAsync(teacherId);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }
}
