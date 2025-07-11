using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LMS.Repositories
{
    public interface IAchievementRepository
    {
        Task<List<AchievementModel>> GetAchievementsAsync();
        Task<List<AchievementModel>> GetAllAchievementsAsync();
        Task<PaginatedResult<AchievementModel>> GetAllAchievementsPaginatedAsync(PaginationRequest request);
        Task<AchievementModel?> GetAchievementByIdAsync(int id);
        Task<AchievementModel> CreateAchievementAsync(CreateAchievementRequest request);
        Task<AchievementModel> UpdateAchievementAsync(int id, CreateAchievementRequest request);
        Task<bool> DeleteAchievementAsync(int id);
        Task<List<UserAchievementModel>> GetUserAchievementsAsync(string userId);
        Task<UserAchievementModel> AwardAchievementAsync(string userId, int achievementId, int? courseId = null);
        Task<bool> RemoveUserAchievementAsync(int userAchievementId);
        Task<int> GetUserTotalPointsAsync(string userId);
    }

    public class AchievementRepository : IAchievementRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public AchievementRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<AchievementModel>> GetAchievementsAsync()
        {
            await using var _context = _contextFactory.CreateDbContext();
            var achievements = await _context.Achievements
                .Include(a => a.UserAchievements)
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return achievements.Select(MapToAchievementModel).ToList();
        }

        public async Task<List<AchievementModel>> GetAllAchievementsAsync()
        {
            await using var _context = _contextFactory.CreateDbContext();
            var achievements = await _context.Achievements
                .Include(a => a.UserAchievements)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return achievements.Select(MapToAchievementModel).ToList();
        }

        public async Task<PaginatedResult<AchievementModel>> GetAllAchievementsPaginatedAsync(PaginationRequest request)
        {
            await using var _context = _contextFactory.CreateDbContext();

            var query = _context.Achievements
                .Include(a => a.UserAchievements)
                .OrderBy(a => a.Name);

            var totalCount = await query.CountAsync();

            var achievements = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginatedResult<AchievementModel>
            {
                Items = achievements.Select(MapToAchievementModel).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<AchievementModel?> GetAchievementByIdAsync(int id)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var achievement = await _context.Achievements
                .Include(a => a.UserAchievements)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

            return achievement != null ? MapToAchievementModel(achievement) : null;
        }

        public async Task<AchievementModel> CreateAchievementAsync(CreateAchievementRequest request)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var achievement = new Achievement
            {
                Name = request.Name,
                Description = request.Description,
                IconUrl = request.IconUrl,
                Points = request.Points,
                BadgeColor = request.BadgeColor,
                Type = Enum.Parse<AchievementType>(request.Type),
                Criteria = request.Criteria,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();

            // Use a new context for retrieval
            return await GetAchievementByIdAsync(achievement.Id) ?? throw new InvalidOperationException("Failed to retrieve created achievement");
        }

        public async Task<AchievementModel> UpdateAchievementAsync(int id, CreateAchievementRequest request)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement == null)
                throw new ArgumentException("Achievement not found", nameof(id));

            achievement.Name = request.Name;
            achievement.Description = request.Description;
            achievement.IconUrl = request.IconUrl;
            achievement.Points = request.Points;
            achievement.BadgeColor = request.BadgeColor;
            achievement.Type = Enum.Parse<AchievementType>(request.Type);
            achievement.Criteria = request.Criteria;

            await _context.SaveChangesAsync();

            return await GetAchievementByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated achievement");
        }

        public async Task<bool> DeleteAchievementAsync(int id)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement == null)
                return false;

            achievement.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserAchievementModel>> GetUserAchievementsAsync(string userId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var userAchievements = await _context.UserAchievements
                .Include(ua => ua.User)
                .Include(ua => ua.Achievement)
                .Include(ua => ua.Course)
                .Where(ua => ua.UserId == userId)
                .OrderByDescending(ua => ua.EarnedAt)
                .ToListAsync();

            return userAchievements.Select(MapToUserAchievementModel).ToList();
        }

        public async Task<UserAchievementModel> AwardAchievementAsync(string userId, int achievementId, int? courseId = null)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var existingAchievement = await _context.UserAchievements
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

            if (existingAchievement != null)
                throw new InvalidOperationException("User already has this achievement");

            var userAchievement = new UserAchievement
            {
                UserId = userId,
                AchievementId = achievementId,
                CourseId = courseId,
                EarnedAt = DateTime.UtcNow
            };

            _context.UserAchievements.Add(userAchievement);
            await _context.SaveChangesAsync();

            var result = await _context.UserAchievements
                .Include(ua => ua.User)
                .Include(ua => ua.Achievement)
                .Include(ua => ua.Course)
                .FirstOrDefaultAsync(ua => ua.Id == userAchievement.Id);

            return result != null ? MapToUserAchievementModel(result) : throw new InvalidOperationException("Failed to retrieve awarded achievement");
        }

        public async Task<bool> RemoveUserAchievementAsync(int userAchievementId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var userAchievement = await _context.UserAchievements.FindAsync(userAchievementId);
            if (userAchievement == null)
                return false;

            _context.UserAchievements.Remove(userAchievement);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUserTotalPointsAsync(string userId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            return await _context.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId)
                .SumAsync(ua => ua.Achievement.Points);
        }

        private static AchievementModel MapToAchievementModel(Achievement achievement)
        {
            return new AchievementModel
            {
                Id = achievement.Id,
                Name = achievement.Name,
                Description = achievement.Description,
                IconUrl = achievement.IconUrl,
                Points = achievement.Points,
                BadgeColor = achievement.BadgeColor,
                Type = achievement.Type.ToString(),
                Criteria = achievement.Criteria?.ToList() ?? new List<AchievementCriteria>(),
                IsActive = achievement.IsActive,
                CreatedAt = achievement.CreatedAt,
                UsersEarnedCount = achievement.UserAchievements?.Count ?? 0
            };
        }

        private static UserAchievementModel MapToUserAchievementModel(UserAchievement userAchievement)
        {
            return new UserAchievementModel
            {
                Id = userAchievement.Id,
                UserId = userAchievement.UserId,
                UserName = userAchievement.User?.UserName ?? "",
                AchievementId = userAchievement.AchievementId,
                AchievementName = userAchievement.Achievement?.Name ?? "",
                AchievementDescription = userAchievement.Achievement?.Description ?? "",
                AchievementIconUrl = userAchievement.Achievement?.IconUrl,
                Points = userAchievement.Achievement?.Points ?? 0,
                BadgeColor = userAchievement.Achievement?.BadgeColor ?? "#ffd700",
                EarnedAt = userAchievement.EarnedAt,
                CourseId = userAchievement.CourseId,
                CourseName = userAchievement.Course?.Title
            };
        }
    }
}
