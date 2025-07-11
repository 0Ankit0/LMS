
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
        group.MapGet("/", async (IAchievementRepository repo) => await repo.GetAchievementsAsync());
        group.MapGet("/all", async (IAchievementRepository repo) => await repo.GetAllAchievementsAsync());
        group.MapPost("/paginated", async (PaginationRequest req, IAchievementRepository repo) => await repo.GetAllAchievementsPaginatedAsync(req));
        group.MapGet("/{id}", async (int id, IAchievementRepository repo) => await repo.GetAchievementByIdAsync(id));
        group.MapPost("/", async (CreateAchievementRequest req, IAchievementRepository repo) => await repo.CreateAchievementAsync(req));
        group.MapPut("/{id}", async (int id, CreateAchievementRequest req, IAchievementRepository repo) => await repo.UpdateAchievementAsync(id, req));
        group.MapDelete("/{id}", async (int id, IAchievementRepository repo) => await repo.DeleteAchievementAsync(id));
        group.MapGet("/user/{userId}", async (string userId, IAchievementRepository repo) => await repo.GetUserAchievementsAsync(userId));
        group.MapPost("/user/{userId}/award", async (string userId, int achievementId, int? courseId, IAchievementRepository repo) => await repo.AwardAchievementAsync(userId, achievementId, courseId));
        group.MapDelete("/user/achievement/{userAchievementId}", async (int userAchievementId, IAchievementRepository repo) => await repo.RemoveUserAchievementAsync(userAchievementId));
        group.MapGet("/user/{userId}/points", async (string userId, IAchievementRepository repo) => await repo.GetUserTotalPointsAsync(userId));
    }
}
