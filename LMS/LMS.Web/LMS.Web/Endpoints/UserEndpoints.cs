
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
        group.MapGet("/", async (IUserRepository repo) => await repo.GetUsersAsync());
        group.MapGet("/{id}", async (string id, IUserRepository repo) => await repo.GetUserByIdAsync(id));
        group.MapPost("/", async (UpdateUserProfileRequest req, IUserRepository repo) => await repo.CreateUserAsync(req));
        group.MapPut("/{id}", async (string id, UpdateUserProfileRequest req, IUserRepository repo) => await repo.UpdateUserAsync(id, req));
        group.MapDelete("/{id}", async (string id, IUserRepository repo) => await repo.DeleteUserAsync(id));
        group.MapPost("/{id}/toggle-status", async (string id, IUserRepository repo) => await repo.ToggleUserStatusAsync(id));
        group.MapGet("/{id}/enrollments", async (string id, IUserRepository repo) => await repo.GetUserEnrollmentsAsync(id));
        group.MapGet("/{id}/achievements", async (string id, IUserRepository repo) => await repo.GetUserAchievementsAsync(id));
        group.MapPost("/{id}/enrollments", async (string id, CreateEnrollmentRequest req, IUserRepository repo) => await repo.CreateEnrollmentAsync(id, req));
        group.MapPut("/{id}/progress", async (string id, UpdateProgressRequest req, IUserRepository repo) => await repo.UpdateProgressAsync(id, req));
    }
}
