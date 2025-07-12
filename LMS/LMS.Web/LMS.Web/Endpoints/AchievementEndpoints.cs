
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class AchievementEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/achievements");
        group.MapGet("/", async (IAchievementRepository repo) => await repo.GetAchievementsAsync())
            .WithName("GetAchievements").WithSummary("Get all achievements for the current user");
        group.MapGet("/all", async (IAchievementRepository repo) => await repo.GetAllAchievementsAsync())
            .WithName("GetAllAchievements").WithSummary("Get all achievements in the system");
        group.MapPost("/paginated", async (PaginationRequest req, IAchievementRepository repo) => await repo.GetAllAchievementsPaginatedAsync(req))
            .WithName("GetAllAchievementsPaginated").WithSummary("Get all achievements with pagination");
        group.MapGet("/{id}", async (int id, IAchievementRepository repo) => await repo.GetAchievementByIdAsync(id))
            .WithName("GetAchievementById").WithSummary("Get achievement by ID");
        group.MapPost("/", async (CreateAchievementRequest req, IAchievementRepository repo) => await repo.CreateAchievementAsync(req))
            .WithName("CreateAchievement").WithSummary("Create a new achievement");
        group.MapPut("/{id}", async (int id, CreateAchievementRequest req, IAchievementRepository repo) => await repo.UpdateAchievementAsync(id, req))
            .WithName("UpdateAchievement").WithSummary("Update an existing achievement");
        group.MapDelete("/{id}", async (int id, IAchievementRepository repo) => await repo.DeleteAchievementAsync(id))
            .WithName("DeleteAchievement").WithSummary("Delete an achievement by ID");
        group.MapGet("/user/{userId}", async (string userId, IAchievementRepository repo) => await repo.GetUserAchievementsAsync(userId))
            .WithName("GetAchievementsForUser").WithSummary("Get all achievements for a specific user");
        group.MapPost("/user/{userId}/award", async (string userId, int achievementId, int? courseId, IAchievementRepository repo) => await repo.AwardAchievementAsync(userId, achievementId, courseId))
            .WithName("AwardAchievement").WithSummary("Award an achievement to a user");
        group.MapDelete("/user/achievement/{userAchievementId}", async (int userAchievementId, IAchievementRepository repo) => await repo.RemoveUserAchievementAsync(userAchievementId))
            .WithName("RemoveUserAchievement").WithSummary("Remove a user's achievement");
        group.MapGet("/user/{userId}/points", async (string userId, IAchievementRepository repo) => await repo.GetUserTotalPointsAsync(userId))
            .WithName("GetUserTotalPoints").WithSummary("Get total achievement points for a user");
    }
}
