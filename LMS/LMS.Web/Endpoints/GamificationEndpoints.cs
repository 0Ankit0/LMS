using LMS.Data.Entities;
using LMS.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Web.Endpoints;

public static class GamificationEndpoints
{
    public static void MapGamificationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/gamification").WithTags("Gamification");

        // Get user achievements
        group.MapGet("/achievements/{userId}", async (string userId, ApplicationDbContext db) =>
        {
            var achievements = await db.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Include(ua => ua.Achievement)
                .Select(ua => new
                {
                    ua.Achievement.Id,
                    ua.Achievement.Name,
                    ua.Achievement.Description,
                    ua.Achievement.IconUrl,
                    ua.Achievement.Points,
                    ua.Achievement.BadgeColor,
                    ua.EarnedAt
                })
                .ToListAsync();

            return Results.Ok(achievements);
        })
        .WithName("GetUserAchievements");

        // Get leaderboard
        group.MapGet("/leaderboard", async (ApplicationDbContext db, int? courseId = null, string period = "all") =>
        {
            var query = db.Users.AsQueryable();

            if (courseId.HasValue)
            {
                query = query.Where(u => u.Enrollments.Any(e => e.CourseId == courseId.Value));
            }

            var leaderboard = await query
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    TotalPoints = u.Achievements.Sum(ua => ua.Achievement.Points),
                    AchievementCount = u.Achievements.Count()
                })
                .OrderByDescending(u => u.TotalPoints)
                .Take(100)
                .ToListAsync();

            return Results.Ok(leaderboard);
        })
        .WithName("GetLeaderboard");

        // Get user points
        group.MapGet("/points/{userId}", async (string userId, ApplicationDbContext db) =>
        {
            var totalPoints = await db.UserAchievements
                .Where(ua => ua.UserId == userId)
                .SumAsync(ua => ua.Achievement.Points);

            var user = await db.Users.FindAsync(userId);
            if (user == null)
                return Results.NotFound();

            return Results.Ok(new { UserId = userId, TotalPoints = totalPoints });
        })
        .WithName("GetUserPoints");

        // Award achievement manually (admin only)
        group.MapPost("/achievements/award", async (AwardAchievementRequest request, ApplicationDbContext db) =>
        {
            // Check if user already has this achievement
            var existingAchievement = await db.UserAchievements
                .FirstOrDefaultAsync(ua => ua.UserId == request.UserId && ua.AchievementId == request.AchievementId);

            if (existingAchievement != null)
                return Results.BadRequest("User already has this achievement");

            var userAchievement = new UserAchievement
            {
                UserId = request.UserId,
                AchievementId = request.AchievementId,
                EarnedAt = DateTime.UtcNow
            };

            db.UserAchievements.Add(userAchievement);
            await db.SaveChangesAsync();

            return Results.Ok(new { Message = "Achievement awarded successfully" });
        })
        .WithName("AwardAchievement");

        // Get all available achievements
        group.MapGet("/achievements", async (ApplicationDbContext db) =>
        {
            var achievements = await db.Achievements
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.Description,
                    a.IconUrl,
                    a.Points,
                    a.BadgeColor,
                    a.IsActive,
                    CriteriaCount = a.Criteria.Count()
                })
                .ToListAsync();

            return Results.Ok(achievements);
        })
        .WithName("GetAllAchievements");

        // Get gamification statistics
        group.MapGet("/stats", async (ApplicationDbContext db) =>
        {
            var stats = new
            {
                TotalAchievements = await db.Achievements.CountAsync(),
                TotalUserAchievements = await db.UserAchievements.CountAsync(),
                TopUser = await db.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        TotalPoints = u.Achievements.Sum(ua => ua.Achievement.Points)
                    })
                    .OrderByDescending(u => u.TotalPoints)
                    .FirstOrDefaultAsync()
            };

            return Results.Ok(stats);
        })
        .WithName("GetGamificationStats");

        // Get user certificates
        group.MapGet("/certificates/{userId}", async (string userId, ApplicationDbContext db) =>
        {
            var certificates = await db.Certificates
                .Where(c => c.UserId == userId)
                .Include(c => c.Course)
                .Select(c => new
                {
                    c.Id,
                    c.CertificateNumber,
                    c.Course.Title,
                    CourseId = c.Course.Id,
                    c.IssuedAt,
                    c.ExpiresAt,
                    c.FinalGrade,
                    c.CertificateUrl,
                    c.IsValid
                })
                .ToListAsync();

            return Results.Ok(certificates);
        })
        .WithName("GetUserCertificates");

        // Get leaderboard entries for a specific leaderboard
        group.MapGet("/leaderboard/{leaderboardId:int}", async (int leaderboardId, ApplicationDbContext db) =>
        {
            var leaderboard = await db.Leaderboards
                .Include(l => l.Entries)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(l => l.Id == leaderboardId);

            if (leaderboard == null)
                return Results.NotFound();

            var entries = leaderboard.Entries
                .OrderBy(e => e.Rank)
                .Select(e => new
                {
                    e.Rank,
                    e.Score,
                    e.User.Id,
                    e.User.FirstName,
                    e.User.LastName,
                    e.LastUpdated
                })
                .ToList();

            return Results.Ok(new
            {
                Leaderboard = new
                {
                    leaderboard.Id,
                    leaderboard.Name,
                    leaderboard.Description,
                    leaderboard.Type,
                    leaderboard.Period
                },
                Entries = entries
            });
        })
        .WithName("GetLeaderboardEntries");
    }
}

// Request DTOs
public record AwardAchievementRequest(string UserId, int AchievementId);
