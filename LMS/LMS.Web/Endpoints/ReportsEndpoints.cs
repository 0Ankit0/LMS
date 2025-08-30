using LMS.Data.DTOs;
using LMS.Web.Infrastructure;
using LMS.Web.Repositories;
using LMS.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LMS.Web.Endpoints
{
    public class ReportsEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/reports").WithTags("Reports");

            group.MapGet("/student-engagement", GetStudentEngagementReport)
                .RequireAuthorization()
                .WithName("GetStudentEngagementReport")
                .WithSummary("Get student engagement report data")
                .WithDescription("Retrieves data for the student engagement report. Access restricted - Admin can access any report, Instructor can only access reports for their courses.");

            group.MapGet("/course-completion", GetCourseCompletionReport)
                .RequireAuthorization()
                .WithName("GetCourseCompletionReport")
                .WithSummary("Get course completion report data")
                .WithDescription("Retrieves data for the course completion report. Access restricted - Admin can access any report, Instructor can only access reports for their courses.");

            group.MapGet("/analytics-dashboard", GetAnalyticsDashboard)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("GetAnalyticsDashboard")
                .WithSummary("Get analytics dashboard data");

            group.MapGet("/custom", GetCustomReport)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("GetCustomReport")
                .WithSummary("Generate custom report");
        }

        private static async Task<IResult> GetStudentEngagementReport(
            [AsParameters] ReportParametersDto parameters,
            IReportRepository reportRepository,
            ClaimsPrincipal user)
        {
            try
            {
                // Check authorization
                var isAdmin = user.IsInRole("Admin");
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!isAdmin && parameters.CourseId.HasValue && !string.IsNullOrEmpty(userId))
                {
                    // Check if instructor owns the course
                    var isInstructor = await reportRepository.IsInstructorOfCourseAsync(userId, parameters.CourseId.Value);
                    if (!isInstructor)
                    {
                        return Results.Forbid();
                    }
                }

                var report = await reportRepository.GetStudentEngagementReportAsync(parameters);
                return Results.Ok(report);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error generating student engagement report: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCourseCompletionReport(
            [AsParameters] ReportParametersDto parameters,
            IReportRepository reportRepository,
            ClaimsPrincipal user)
        {
            try
            {
                // Check authorization
                var isAdmin = user.IsInRole("Admin");
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!isAdmin && parameters.CourseId.HasValue && !string.IsNullOrEmpty(userId))
                {
                    // Check if instructor owns the course
                    var isInstructor = await reportRepository.IsInstructorOfCourseAsync(userId, parameters.CourseId.Value);
                    if (!isInstructor)
                    {
                        return Results.Forbid();
                    }
                }

                var report = await reportRepository.GetCourseCompletionReportAsync(parameters);
                return Results.Ok(report);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error generating course completion report: {ex.Message}");
            }
        }

        private static async Task<IResult> GetAnalyticsDashboard(
            IReportRepository reportRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                var isAdmin = user.IsInRole("Admin");

                var dashboard = await reportRepository.GetAnalyticsDashboardAsync(userId, isAdmin);
                return Results.Ok(dashboard);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error loading analytics dashboard: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCustomReport(
            [AsParameters] CustomReportParametersDto parameters,
            IReportRepository reportRepository)
        {
            try
            {
                var report = await reportRepository.GenerateCustomReportAsync(parameters);
                return Results.Ok(report);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error generating custom report: {ex.Message}");
            }
        }
    }
}
