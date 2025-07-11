
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class ProgressEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/progress");
        group.MapGet("/module/{enrollmentId}", async (int enrollmentId, IProgressRepository repo) => await repo.GetModuleProgressAsync(enrollmentId));
        group.MapGet("/lesson/{enrollmentId}", async (int enrollmentId, IProgressRepository repo) => await repo.GetLessonProgressAsync(enrollmentId));
        group.MapGet("/module/{enrollmentId}/{moduleId}", async (int enrollmentId, int moduleId, IProgressRepository repo) => await repo.GetModuleProgressAsync(enrollmentId, moduleId));
        group.MapGet("/lesson/{enrollmentId}/{lessonId}", async (int enrollmentId, int lessonId, IProgressRepository repo) => await repo.GetLessonProgressAsync(enrollmentId, lessonId));
        group.MapPut("/module", async (UpdateProgressRequest req, IProgressRepository repo) => await repo.UpdateModuleProgressAsync(req));
        group.MapPut("/lesson", async (UpdateProgressRequest req, IProgressRepository repo) => await repo.UpdateLessonProgressAsync(req));
        group.MapGet("/user/{userId}/achievements", async (string userId, IProgressRepository repo) => await repo.GetUserAchievementsAsync(userId));
        group.MapPost("/user/{userId}/achievement", async (string userId, string type, string title, string description, int? points, IProgressRepository repo) => await repo.AddAchievementAsync(userId, type, title, description, points ?? 0));
        group.MapGet("/leaderboard", async (int? limit, IProgressRepository repo) => await repo.GetLeaderboardAsync(limit ?? 10));
        group.MapGet("/leaderboard/user/{userId}", async (string userId, IProgressRepository repo) => await repo.GetUserLeaderboardPositionAsync(userId));
        group.MapPost("/leaderboard/update", async (IProgressRepository repo) => { await repo.UpdateLeaderboardAsync(); return Results.Ok(); });
        group.MapGet("/summary/{enrollmentId}", async (int enrollmentId, IProgressRepository repo) => await repo.GetProgressSummaryAsync(enrollmentId));
    }
}
