
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
        group.MapGet("/", async (ILeaderboardRepository repo) => await repo.GetLeaderboardsAsync());
        group.MapGet("/{id}", async (int id, ILeaderboardRepository repo) => await repo.GetLeaderboardByIdAsync(id));
        group.MapGet("/course/{courseId}", async (int courseId, ILeaderboardRepository repo) => await repo.GetLeaderboardByCourseIdAsync(courseId));
        group.MapGet("/{leaderboardId}/entries", async (int leaderboardId, int? limit, ILeaderboardRepository repo) => await repo.GetLeaderboardEntriesAsync(leaderboardId, limit ?? 50));
        group.MapGet("/global", async (int? limit, ILeaderboardRepository repo) => await repo.GetGlobalLeaderboardAsync(limit ?? 50));
        group.MapGet("/course/{courseId}/entries", async (int courseId, int? limit, ILeaderboardRepository repo) => await repo.GetCourseLeaderboardAsync(courseId, limit ?? 50));
        group.MapGet("/weekly", async (int? limit, ILeaderboardRepository repo) => await repo.GetWeeklyLeaderboardAsync(limit ?? 50));
        group.MapGet("/monthly", async (int? limit, ILeaderboardRepository repo) => await repo.GetMonthlyLeaderboardAsync(limit ?? 50));
        group.MapGet("/overall", async (int? limit, ILeaderboardRepository repo) => await repo.GetOverallLeaderboardAsync(limit ?? 50));
        group.MapPost("/user/{userId}/course/{courseId}/score", async (string userId, int courseId, double score, ILeaderboardRepository repo) => { await repo.UpdateUserScoreAsync(userId, courseId, score); return Results.Ok(); });
        group.MapGet("/user/{userId}/rank", async (string userId, int? courseId, ILeaderboardRepository repo) => await repo.GetUserRankAsync(userId, courseId));
    }
}
