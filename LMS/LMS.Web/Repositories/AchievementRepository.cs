using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LMS.Web.Repositories
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
        Task<UserAchievementsModel> GetUserAchievementsDataAsync(string userId);
    }

    public class AchievementRepository : IAchievementRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<AchievementRepository> _logger;

        public AchievementRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<AchievementRepository> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<AchievementModel>> GetAchievementsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var achievements = await context.Achievements
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
                using var context = _contextFactory.CreateDbContext();
                var achievements = await context.Achievements
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
                using var context = _contextFactory.CreateDbContext();
                var query = context.Achievements
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
                using var context = _contextFactory.CreateDbContext();
                var achievement = await context.Achievements
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
                using var context = _contextFactory.CreateDbContext();
                var achievement = new Achievement
                {
                    Name = request.Name,
                    Description = request.Description,
                    IconUrl = request.IconUrl,
                    Points = request.Points,
                    BadgeColor = request.BadgeColor,
                    Type = Enum.Parse<AchievementType>(request.Type),
                    Criteria = request.Criteria?.Select(c => new LMS.Data.Entities.AchievementCriteria
                    {
                        Type = (LMS.Data.Entities.CriteriaType)c.Type,
                        AdditionalData = c.TargetValue,
                        CourseId = c.Parameters.ContainsKey("CourseId") ? (int?)c.Parameters["CourseId"] : null,
                        AssessmentId = c.Parameters.ContainsKey("AssessmentId") ? (int?)c.Parameters["AssessmentId"] : null,
                        MinScore = c.Parameters.ContainsKey("MinScore") ? (double?)c.Parameters["MinScore"] : null,
                        RequiredCount = c.Parameters.ContainsKey("RequiredCount") ? (int?)c.Parameters["RequiredCount"] : null
                    }).ToList() ?? new List<LMS.Data.Entities.AchievementCriteria>(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Achievements.Add(achievement);
                await context.SaveChangesAsync();

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
                using var context = _contextFactory.CreateDbContext();
                var achievement = await context.Achievements.FindAsync(id);
                if (achievement == null)
                    throw new ArgumentException("Achievement not found", nameof(id));

                achievement.Name = request.Name;
                achievement.Description = request.Description;
                achievement.IconUrl = request.IconUrl;
                achievement.Points = request.Points;
                achievement.BadgeColor = request.BadgeColor;
                achievement.Type = Enum.Parse<AchievementType>(request.Type);

                // Clear existing criteria and add new ones
                achievement.Criteria.Clear();
                if (request.Criteria != null)
                {
                    foreach (var criteriaDto in request.Criteria)
                    {
                        achievement.Criteria.Add(new LMS.Data.Entities.AchievementCriteria
                        {
                            Type = (LMS.Data.Entities.CriteriaType)criteriaDto.Type,
                            AdditionalData = criteriaDto.TargetValue,
                            CourseId = criteriaDto.Parameters.ContainsKey("CourseId") ? (int?)criteriaDto.Parameters["CourseId"] : null,
                            AssessmentId = criteriaDto.Parameters.ContainsKey("AssessmentId") ? (int?)criteriaDto.Parameters["AssessmentId"] : null,
                            MinScore = criteriaDto.Parameters.ContainsKey("MinScore") ? (double?)criteriaDto.Parameters["MinScore"] : null,
                            RequiredCount = criteriaDto.Parameters.ContainsKey("RequiredCount") ? (int?)criteriaDto.Parameters["RequiredCount"] : null
                        });
                    }
                }

                await context.SaveChangesAsync();

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
                using var context = _contextFactory.CreateDbContext();
                var achievement = await context.Achievements.FindAsync(id);
                if (achievement == null)
                    return false;

                achievement.IsActive = false;
                await context.SaveChangesAsync();
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
                using var context = _contextFactory.CreateDbContext();
                var userAchievements = await context.UserAchievements
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
                using var context = _contextFactory.CreateDbContext();
                var existingAchievement = await context.UserAchievements
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

                context.UserAchievements.Add(userAchievement);
                await context.SaveChangesAsync();

                var result = await context.UserAchievements
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
                using var context = _contextFactory.CreateDbContext();
                var userAchievement = await context.UserAchievements.FindAsync(userAchievementId);
                if (userAchievement == null)
                    return false;

                context.UserAchievements.Remove(userAchievement);
                await context.SaveChangesAsync();
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
                using var context = _contextFactory.CreateDbContext();
                return await context.UserAchievements
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
                Criteria = achievement.Criteria?.Select(c => new LMS.Data.DTOs.AchievementCriteria
                {
                    Id = c.Id,
                    AchievementId = c.AchievementId,
                    Type = (LMS.Data.DTOs.CriteriaType)c.Type,
                    TargetValue = c.AdditionalData,
                    ComparisonOperator = ">=", // Default operator
                    Parameters = new Dictionary<string, object>
                    {
                        { "CourseId", c.CourseId ?? 0 },
                        { "AssessmentId", c.AssessmentId ?? 0 },
                        { "MinScore", c.MinScore ?? 0.0 },
                        { "RequiredCount", c.RequiredCount ?? 0 }
                    }
                }).ToList() ?? new List<LMS.Data.DTOs.AchievementCriteria>(),
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

        public async Task<UserAchievementsModel> GetUserAchievementsDataAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // Get earned achievements
                var earnedAchievements = await GetUserAchievementsAsync(userId);

                // Get available achievements (not yet earned)
                var allAchievements = await GetAllAchievementsAsync();
                var earnedIds = earnedAchievements.Select(ea => ea.AchievementId).ToHashSet();
                var availableAchievements = allAchievements.Where(a => !earnedIds.Contains(a.Id)).ToList();

                // Get total points
                var totalPoints = await GetUserTotalPointsAsync(userId);

                // Get leaderboard data (to be implemented when leaderboard functionality is added)
                var leaderboardEntries = new List<LeaderboardEntryModel>();

                // Calculate user rank (to be implemented when ranking system is added)
                var userRank = 1;

                return new UserAchievementsModel
                {
                    EarnedAchievements = earnedAchievements,
                    AvailableAchievements = availableAchievements,
                    LeaderboardEntries = leaderboardEntries,
                    TotalPoints = totalPoints,
                    EarnedCount = earnedAchievements.Count,
                    AvailableCount = availableAchievements.Count,
                    UserRank = userRank
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user achievements data for user {UserId}", userId);

                // Return empty model on error
                return new UserAchievementsModel();
            }
        }
    }
}
