
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
        group.MapGet("/", async (IEnrollmentRepository repo) => await repo.GetEnrollmentsAsync());
        group.MapPost("/paginated", async (PaginationRequest req, IEnrollmentRepository repo) => await repo.GetEnrollmentsPaginatedAsync(req));
        group.MapGet("/{id}", async (int id, IEnrollmentRepository repo) => await repo.GetEnrollmentByIdAsync(id));
        group.MapGet("/user/{userId}", async (string userId, IEnrollmentRepository repo) => await repo.GetEnrollmentsByUserIdAsync(userId));
        group.MapGet("/user/{userId}/enrollments", async (string userId, IEnrollmentRepository repo) => await repo.GetUserEnrollmentsAsync(userId));
        group.MapGet("/course/{courseId}", async (int courseId, IEnrollmentRepository repo) => await repo.GetEnrollmentsByCourseIdAsync(courseId));
        group.MapGet("/user/{userId}/course/{courseId}", async (string userId, int courseId, IEnrollmentRepository repo) => await repo.GetEnrollmentByUserAndCourseAsync(userId, courseId));
        group.MapPost("/user/{userId}", async (string userId, CreateEnrollmentRequest req, IEnrollmentRepository repo) => await repo.CreateEnrollmentAsync(userId, req));
        group.MapPut("/{id}/status", async (int id, string status, IEnrollmentRepository repo) => await repo.UpdateEnrollmentStatusAsync(id, status));
        group.MapDelete("/{id}", async (int id, IEnrollmentRepository repo) => await repo.DeleteEnrollmentAsync(id));
        group.MapDelete("/user/{userId}/course/{courseId}", async (string userId, int courseId, IEnrollmentRepository repo) => await repo.UnenrollUserAsync(userId, courseId));
        group.MapGet("/user/{userId}/course/{courseId}/isenrolled", async (string userId, int courseId, IEnrollmentRepository repo) => await repo.IsUserEnrolledInCourseAsync(userId, courseId));
        group.MapGet("/course/{courseId}/count", async (int courseId, IEnrollmentRepository repo) => await repo.GetCourseEnrollmentCountAsync(courseId));
    }
}
