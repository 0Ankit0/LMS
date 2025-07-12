
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class LeaderboardEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leaderboards");
        group.MapGet("/", async (ILeaderboardRepository repo) => await repo.GetLeaderboardsAsync())
            .WithName("GetLeaderboards").WithSummary("Get all leaderboards");
        group.MapGet("/{id}", async (int id, ILeaderboardRepository repo) => await repo.GetLeaderboardByIdAsync(id))
            .WithName("GetLeaderboardById").WithSummary("Get leaderboard by ID");
        group.MapGet("/course/{courseId}", async (int courseId, ILeaderboardRepository repo) => await repo.GetLeaderboardByCourseIdAsync(courseId))
            .WithName("GetLeaderboardByCourseId").WithSummary("Get leaderboard by course ID");
        group.MapGet("/{leaderboardId}/entries", async (int leaderboardId, int? limit, ILeaderboardRepository repo) => await repo.GetLeaderboardEntriesAsync(leaderboardId, limit ?? 50))
            .WithName("GetLeaderboardEntries").WithSummary("Get leaderboard entries by leaderboard ID");
        group.MapGet("/global", async (int? limit, ILeaderboardRepository repo) => await repo.GetGlobalLeaderboardAsync(limit ?? 50))
            .WithName("GetGlobalLeaderboard").WithSummary("Get the global leaderboard");
        group.MapGet("/course/{courseId}/entries", async (int courseId, int? limit, ILeaderboardRepository repo) => await repo.GetCourseLeaderboardAsync(courseId, limit ?? 50))
            .WithName("GetCourseLeaderboard").WithSummary("Get the leaderboard for a course");
        group.MapGet("/weekly", async (int? limit, ILeaderboardRepository repo) => await repo.GetWeeklyLeaderboardAsync(limit ?? 50))
            .WithName("GetWeeklyLeaderboard").WithSummary("Get the weekly leaderboard");
        group.MapGet("/monthly", async (int? limit, ILeaderboardRepository repo) => await repo.GetMonthlyLeaderboardAsync(limit ?? 50))
            .WithName("GetMonthlyLeaderboard").WithSummary("Get the monthly leaderboard");
        group.MapGet("/overall", async (int? limit, ILeaderboardRepository repo) => await repo.GetOverallLeaderboardAsync(limit ?? 50))
            .WithName("GetOverallLeaderboard").WithSummary("Get the overall leaderboard");
        group.MapPost("/user/{userId}/course/{courseId}/score", async (string userId, int courseId, double score, ILeaderboardRepository repo) => { await repo.UpdateUserScoreAsync(userId, courseId, score); return Results.Ok(); })
            .WithName("UpdateUserScore").WithSummary("Update a user's score for a course");
        group.MapGet("/user/{userId}/rank", async (string userId, int? courseId, ILeaderboardRepository repo) => await repo.GetUserRankAsync(userId, courseId))
            .WithName("GetUserRank").WithSummary("Get a user's rank in a leaderboard");
    }
}
