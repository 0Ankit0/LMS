
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class EnrollmentEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/enrollments");
        group.MapGet("/", async (IEnrollmentRepository repo) => await repo.GetEnrollmentsAsync())
            .WithName("GetEnrollments").WithSummary("Get all enrollments");
        group.MapPost("/paginated", async (PaginationRequest req, IEnrollmentRepository repo) => await repo.GetEnrollmentsPaginatedAsync(req))
            .WithName("GetEnrollmentsPaginated").WithSummary("Get enrollments with pagination");
        group.MapGet("/{id}", async (int id, IEnrollmentRepository repo) => await repo.GetEnrollmentByIdAsync(id))
            .WithName("GetEnrollmentById").WithSummary("Get enrollment by ID");
        group.MapGet("/user/{userId}", async (string userId, IEnrollmentRepository repo) => await repo.GetEnrollmentsByUserIdAsync(userId))
            .WithName("GetEnrollmentsByUserId").WithSummary("Get enrollments by user ID");
        group.MapGet("/user/{userId}/enrollments", async (string userId, IEnrollmentRepository repo) => await repo.GetUserEnrollmentsAsync(userId))
            .WithName("GetEnrollmentsForUser").WithSummary("Get all enrollments for a user");
        group.MapGet("/course/{courseId}", async (int courseId, IEnrollmentRepository repo) => await repo.GetEnrollmentsByCourseIdAsync(courseId))
            .WithName("GetEnrollmentsByCourseId").WithSummary("Get enrollments by course ID");
        group.MapGet("/user/{userId}/course/{courseId}", async (string userId, int courseId, IEnrollmentRepository repo) => await repo.GetEnrollmentByUserAndCourseAsync(userId, courseId))
            .WithName("GetEnrollmentByUserAndCourse").WithSummary("Get enrollment by user and course");
        group.MapPost("/user/{userId}", async (string userId, CreateEnrollmentRequest req, IEnrollmentRepository repo) => await repo.CreateEnrollmentAsync(userId, req))
            .WithName("CreateEnrollment").WithSummary("Create a new enrollment for a user");
        group.MapPut("/{id}/status", async (int id, string status, IEnrollmentRepository repo) => await repo.UpdateEnrollmentStatusAsync(id, status))
            .WithName("UpdateEnrollmentStatus").WithSummary("Update the status of an enrollment");
        group.MapDelete("/{id}", async (int id, IEnrollmentRepository repo) => await repo.DeleteEnrollmentAsync(id))
            .WithName("DeleteEnrollment").WithSummary("Delete an enrollment by ID");
        group.MapDelete("/user/{userId}/course/{courseId}", async (string userId, int courseId, IEnrollmentRepository repo) => await repo.UnenrollUserAsync(userId, courseId))
            .WithName("UnenrollUser").WithSummary("Unenroll a user from a course");
        group.MapGet("/user/{userId}/course/{courseId}/isenrolled", async (string userId, int courseId, IEnrollmentRepository repo) => await repo.IsUserEnrolledInCourseAsync(userId, courseId))
            .WithName("IsUserEnrolledInCourse").WithSummary("Check if a user is enrolled in a course");
        group.MapGet("/course/{courseId}/count", async (int courseId, IEnrollmentRepository repo) => await repo.GetCourseEnrollmentCountAsync(courseId))
            .WithName("GetCourseEnrollmentCount").WithSummary("Get enrollment count for a course");
    }
}
