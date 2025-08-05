using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Data;
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
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AchievementRepository> _logger;

        public AchievementRepository(ApplicationDbContext context, ILogger<AchievementRepository> logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<AchievementModel>> GetAchievementsAsync()
        {
            try
            {
                var achievements = await _context.Achievements
                    .Include(a => a.UserAchievements)
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return achievements.Select(MapToAchievementModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting achievements");
                throw;
            }
        }

        public async Task<List<AchievementModel>> GetAllAchievementsAsync()
        {
            try
            {
                var achievements = await _context.Achievements
                    .Include(a => a.UserAchievements)
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return achievements.Select(MapToAchievementModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all achievements");
                throw;
            }
        }

        public async Task<PaginatedResult<AchievementModel>> GetAllAchievementsPaginatedAsync(PaginationRequest request)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated achievements");
                throw;
            }
        }

        public async Task<AchievementModel?> GetAchievementByIdAsync(int id)
        {
            try
            {
                var achievement = await _context.Achievements
                    .Include(a => a.UserAchievements)
                    .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

                return achievement != null ? MapToAchievementModel(achievement) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting achievement by id: {AchievementId}", id);
                throw;
            }
        }

        public async Task<AchievementModel> CreateAchievementAsync(CreateAchievementRequest request)
        {
            try
            {
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

                return await GetAchievementByIdAsync(achievement.Id) ?? throw new InvalidOperationException("Failed to retrieve created achievement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating achievement");
                throw;
            }
        }

        public async Task<AchievementModel> UpdateAchievementAsync(int id, CreateAchievementRequest request)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating achievement: {AchievementId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAchievementAsync(int id)
        {
            try
            {
                var achievement = await _context.Achievements.FindAsync(id);
                if (achievement == null)
                    return false;

                achievement.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting achievement: {AchievementId}", id);
                throw;
            }
        }

        public async Task<List<UserAchievementModel>> GetUserAchievementsAsync(string userId)
        {
            try
            {
                var userAchievements = await _context.UserAchievements
                    .Include(ua => ua.User)
                    .Include(ua => ua.Achievement)
                    .Include(ua => ua.Course)
                    .Where(ua => ua.UserId == userId)
                    .OrderByDescending(ua => ua.EarnedAt)
                    .ToListAsync();

                return userAchievements.Select(MapToUserAchievementModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user achievements for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<UserAchievementModel> AwardAchievementAsync(string userId, int achievementId, int? courseId = null)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding achievement: {AchievementId} to user: {UserId}", achievementId, userId);
                throw;
            }
        }

        public async Task<bool> RemoveUserAchievementAsync(int userAchievementId)
        {
            try
            {
                var userAchievement = await _context.UserAchievements.FindAsync(userAchievementId);
                if (userAchievement == null)
                    return false;

                _context.UserAchievements.Remove(userAchievement);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user achievement: {UserAchievementId}", userAchievementId);
                throw;
            }
        }

        public async Task<int> GetUserTotalPointsAsync(string userId)
        {
            try
            {
                return await _context.UserAchievements
                    .Include(ua => ua.Achievement)
                    .Where(ua => ua.UserId == userId)
                    .SumAsync(ua => ua.Achievement.Points);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total points for user: {UserId}", userId);
                throw;
            }
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
