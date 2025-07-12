
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
        group.MapGet("/module/{enrollmentId}", async (int enrollmentId, IProgressRepository repo) => await repo.GetModuleProgressAsync(enrollmentId))
            .WithName("GetModuleProgress").WithSummary("Get progress for a module by enrollment ID");
        group.MapGet("/lesson/{enrollmentId}", async (int enrollmentId, IProgressRepository repo) => await repo.GetLessonProgressAsync(enrollmentId))
            .WithName("GetLessonProgress").WithSummary("Get progress for a lesson by enrollment ID");
        group.MapGet("/module/{enrollmentId}/{moduleId}", async (int enrollmentId, int moduleId, IProgressRepository repo) => await repo.GetModuleProgressAsync(enrollmentId, moduleId))
            .WithName("GetModuleProgressByModule").WithSummary("Get progress for a specific module in an enrollment");
        group.MapGet("/lesson/{enrollmentId}/{lessonId}", async (int enrollmentId, int lessonId, IProgressRepository repo) => await repo.GetLessonProgressAsync(enrollmentId, lessonId))
            .WithName("GetLessonProgressByLesson").WithSummary("Get progress for a specific lesson in an enrollment");
        group.MapPut("/module", async (UpdateProgressRequest req, IProgressRepository repo) => await repo.UpdateModuleProgressAsync(req))
            .WithName("UpdateModuleProgress").WithSummary("Update progress for a module");
        group.MapPut("/lesson", async (UpdateProgressRequest req, IProgressRepository repo) => await repo.UpdateLessonProgressAsync(req))
            .WithName("UpdateLessonProgress").WithSummary("Update progress for a lesson");
        group.MapGet("/user/{userId}/achievements", async (string userId, IProgressRepository repo) => await repo.GetUserAchievementsAsync(userId))
            .WithName("GetUserAchievementsProgress").WithSummary("Get achievements for a user in progress context");
        group.MapPost("/user/{userId}/achievement", async (string userId, string type, string title, string description, int? points, IProgressRepository repo) => await repo.AddAchievementAsync(userId, type, title, description, points ?? 0))
            .WithName("AddUserAchievement").WithSummary("Add an achievement for a user");
        group.MapGet("/leaderboard", async (int? limit, IProgressRepository repo) => await repo.GetLeaderboardAsync(limit ?? 10))
            .WithName("GetLeaderboard").WithSummary("Get the leaderboard");
        group.MapGet("/leaderboard/user/{userId}", async (string userId, IProgressRepository repo) => await repo.GetUserLeaderboardPositionAsync(userId))
            .WithName("GetUserLeaderboardPosition").WithSummary("Get a user's position in the leaderboard");
        group.MapPost("/leaderboard/update", async (IProgressRepository repo) => { await repo.UpdateLeaderboardAsync(); return Results.Ok(); })
            .WithName("UpdateLeaderboard").WithSummary("Update the leaderboard");
        group.MapGet("/summary/{enrollmentId}", async (int enrollmentId, IProgressRepository repo) => await repo.GetProgressSummaryAsync(enrollmentId))
            .WithName("GetProgressSummary").WithSummary("Get progress summary for an enrollment");
    }
}
