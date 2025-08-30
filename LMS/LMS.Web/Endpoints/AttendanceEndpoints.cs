using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Infrastructure;
using LMS.Web.Repositories;
using LMS.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LMS.Web.Endpoints
{
    public class AttendanceEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/attendance").WithTags("Attendance");

            group.MapPost("", SubmitBatchAttendance)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("SubmitBatchAttendance")
                .WithSummary("Submit attendance for multiple students")
                .WithDescription("Batch endpoint for an instructor or admin to submit attendance for multiple students for a specific class session/date");

            group.MapGet("/courses/{courseId:int}", GetCourseAttendance)
                .RequireAuthorization()
                .WithName("GetCourseAttendance")
                .WithSummary("Get all attendance records for a specific course")
                .WithDescription("Retrieves all attendance records for a specific course. Authorization: Admin or course Instructor");

            group.MapGet("/users/{userId}", GetUserAttendance)
                .RequireAuthorization()
                .WithName("GetUserAttendance")
                .WithSummary("Get all attendance records for a specific user")
                .WithDescription("Retrieves all attendance records for a specific user (student or instructor). Authorization: Admin, or the user themselves");

            group.MapPut("/{id:int}", UpdateAttendanceRecord)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("UpdateAttendanceRecord")
                .WithSummary("Update a single attendance record")
                .WithDescription("Updates a single, existing attendance record. Authorization: Admin or Instructor");

            group.MapGet("/summary/{userId}", GetAttendanceSummary)
                .RequireAuthorization()
                .WithName("GetAttendanceSummary")
                .WithSummary("Get attendance summary for a user");

            group.MapGet("/courses/{courseId:int}/sessions", GetCourseSessions)
                .RequireAuthorization()
                .WithName("GetCourseSessions")
                .WithSummary("Get all sessions for a course");
        }

        private static async Task<IResult> SubmitBatchAttendance(
            BatchAttendanceDto batchAttendance,
            IAttendanceRepository attendanceRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var isAdmin = user.IsInRole("Admin");

                // Check if user can submit attendance for this course
                if (!isAdmin)
                {
                    var isInstructor = await attendanceRepository.IsInstructorOfCourseAsync(userId, batchAttendance.CourseId);
                    if (!isInstructor)
                    {
                        return Results.Forbid();
                    }
                }

                var result = await attendanceRepository.SubmitBatchAttendanceAsync(batchAttendance, userId);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error submitting batch attendance: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCourseAttendance(
            int courseId,
            IAttendanceRepository attendanceRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var isAdmin = user.IsInRole("Admin");

                // Check authorization
                if (!isAdmin)
                {
                    var isInstructor = await attendanceRepository.IsInstructorOfCourseAsync(userId, courseId);
                    if (!isInstructor)
                    {
                        return Results.Forbid();
                    }
                }

                var attendance = await attendanceRepository.GetCourseAttendanceAsync(courseId);
                return Results.Ok(attendance);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving course attendance: {ex.Message}");
            }
        }

        private static async Task<IResult> GetUserAttendance(
            string userId,
            IAttendanceRepository attendanceRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = user.IsInRole("Admin");

                // Check authorization - Admin or the user themselves
                if (!isAdmin && currentUserId != userId)
                {
                    return Results.Forbid();
                }

                var attendance = await attendanceRepository.GetUserAttendanceAsync(userId);
                return Results.Ok(attendance);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving user attendance: {ex.Message}");
            }
        }

        private static async Task<IResult> UpdateAttendanceRecord(
            int id,
            UpdateAttendanceDto updateAttendance,
            IAttendanceRepository attendanceRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var isAdmin = user.IsInRole("Admin");

                // Check if user can update this attendance record
                if (!isAdmin)
                {
                    var canUpdate = await attendanceRepository.CanUserUpdateAttendanceAsync(userId, id);
                    if (!canUpdate)
                    {
                        return Results.Forbid();
                    }
                }

                var result = await attendanceRepository.UpdateAttendanceRecordAsync(id, updateAttendance, userId);
                if (result is null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating attendance record: {ex.Message}");
            }
        }

        private static async Task<IResult> GetAttendanceSummary(
            string userId,
            IAttendanceRepository attendanceRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = user.IsInRole("Admin");

                // Check authorization
                if (!isAdmin && currentUserId != userId)
                {
                    return Results.Forbid();
                }

                var summary = await attendanceRepository.GetAttendanceSummaryAsync(userId);
                return Results.Ok(summary);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving attendance summary: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCourseSessions(
            int courseId,
            IAttendanceRepository attendanceRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var isAdmin = user.IsInRole("Admin");

                // Check authorization
                if (!isAdmin)
                {
                    var hasAccess = await attendanceRepository.HasCourseAccessAsync(userId, courseId);
                    if (!hasAccess)
                    {
                        return Results.Forbid();
                    }
                }

                var sessions = await attendanceRepository.GetCourseSessionsAsync(courseId);
                return Results.Ok(sessions);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving course sessions: {ex.Message}");
            }
        }
    }
}
