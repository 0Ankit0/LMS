
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class UserEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");
        group.MapGet("/", async (IUserRepository repo) => await repo.GetUsersAsync())
            .WithName("GetUsers").WithSummary("Get all users");
        group.MapGet("/{id}", async (string id, IUserRepository repo) => await repo.GetUserByIdAsync(id))
            .WithName("GetUserById").WithSummary("Get user by ID");
        group.MapPost("/", async (UpdateUserProfileRequest req, IUserRepository repo) => await repo.CreateUserAsync(req))
            .WithName("CreateUser").WithSummary("Create a new user");
        group.MapPut("/{id}", async (string id, UpdateUserProfileRequest req, IUserRepository repo) => await repo.UpdateUserAsync(id, req))
            .WithName("UpdateUser").WithSummary("Update a user");
        group.MapDelete("/{id}", async (string id, IUserRepository repo) => await repo.DeleteUserAsync(id))
            .WithName("DeleteUser").WithSummary("Delete a user by ID");
        group.MapPost("/{id}/toggle-status", async (string id, IUserRepository repo) => await repo.ToggleUserStatusAsync(id))
            .WithName("ToggleUserStatus").WithSummary("Toggle the status of a user");
        group.MapGet("/{id}/enrollments", async (string id, IUserRepository repo) => await repo.GetUserEnrollmentsAsync(id))
            .WithName("GetUserEnrollments").WithSummary("Get all enrollments for a user");
        group.MapGet("/{id}/achievements", async (string id, IUserRepository repo) => await repo.GetUserAchievementsAsync(id))
            .WithName("GetUserAchievements").WithSummary("Get all achievements for a user");
        group.MapPost("/{id}/enrollments", async (string id, CreateEnrollmentRequest req, IUserRepository repo) => await repo.CreateEnrollmentAsync(id, req))
            .WithName("CreateEnrollmentForUser").WithSummary("Create an enrollment for a user");
        group.MapPut("/{id}/progress", async (string id, UpdateProgressRequest req, IUserRepository repo) => await repo.UpdateProgressAsync(id, req))
            .WithName("UpdateUserProgress").WithSummary("Update progress for a user");
    }
}
