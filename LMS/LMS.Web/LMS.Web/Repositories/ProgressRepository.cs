using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using LMS.Infrastructure.Data;
using LMS.Data.DTOs;

namespace LMS.Repositories;

public interface IProgressRepository
{
    Task<IEnumerable<ModuleProgress>> GetModuleProgressAsync(int enrollmentId);
    Task<IEnumerable<LessonProgress>> GetLessonProgressAsync(int enrollmentId);
    Task<ModuleProgress?> GetModuleProgressAsync(int enrollmentId, int moduleId);
    Task<LessonProgress?> GetLessonProgressAsync(int enrollmentId, int lessonId);
    Task<ModuleProgress> UpdateModuleProgressAsync(UpdateProgressRequest request);
    Task<LessonProgress> UpdateLessonProgressAsync(UpdateProgressRequest request);
    Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(string userId);
    Task<UserAchievement> AddAchievementAsync(string userId, string type, string title, string description, int points = 0);
    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int limit = 10);
    Task<LeaderboardEntry?> GetUserLeaderboardPositionAsync(string userId);
    Task UpdateLeaderboardAsync();
    Task<Dictionary<string, object>> GetProgressSummaryAsync(int enrollmentId);
}
public class ProgressRepository : IProgressRepository
{

    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProgressRepository> _logger;

    public ProgressRepository(ApplicationDbContext context, ILogger<ProgressRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ModuleProgress>> GetModuleProgressAsync(int enrollmentId)
    {
        try
        {
            return await _context.ModuleProgresses
                .Where(mp => mp.EnrollmentId == enrollmentId)
                .Include(mp => mp.Module)
                .Include(mp => mp.Enrollment)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting module progress for enrollment: {EnrollmentId}", enrollmentId);
            throw;
        }
    }

    public async Task<IEnumerable<LessonProgress>> GetLessonProgressAsync(int enrollmentId)
    {
        try
        {
            return await _context.LessonProgresses
                .Where(lp => lp.ModuleProgress.EnrollmentId == enrollmentId)
                .Include(lp => lp.Lesson)
                .Include(lp => lp.ModuleProgress)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lesson progress for enrollment: {EnrollmentId}", enrollmentId);
            throw;
        }
    }

    public async Task<ModuleProgress?> GetModuleProgressAsync(int enrollmentId, int moduleId)
    {
        try
        {
            return await _context.ModuleProgresses
                .FirstOrDefaultAsync(mp => mp.EnrollmentId == enrollmentId && mp.ModuleId == moduleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting module progress for enrollment: {EnrollmentId}, module: {ModuleId}", enrollmentId, moduleId);
            throw;
        }
    }

    public async Task<LessonProgress?> GetLessonProgressAsync(int enrollmentId, int lessonId)
    {
        try
        {
            return await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.ModuleProgress.EnrollmentId == enrollmentId && lp.LessonId == lessonId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lesson progress for enrollment: {EnrollmentId}, lesson: {LessonId}", enrollmentId, lessonId);
            throw;
        }
    }

    public async Task<ModuleProgress> UpdateModuleProgressAsync(UpdateProgressRequest request)
    {
        try
        {
            var progress = await GetModuleProgressAsync(request.EnrollmentId, request.ModuleId ?? 0);

            if (progress == null)
            {
                progress = new ModuleProgress
                {
                    EnrollmentId = request.EnrollmentId,
                    ModuleId = request.ModuleId ?? 0,
                    StartedAt = DateTime.UtcNow,
                    ProgressPercentage = request.ProgressPercentage,
                    TimeSpent = TimeSpan.FromMinutes(request.TimeSpentMinutes ?? 0)
                };

                if (request.IsCompleted)
                {
                    progress.CompletedAt = DateTime.UtcNow;
                }

                _context.ModuleProgresses.Add(progress);
            }
            else
            {
                progress.ProgressPercentage = request.ProgressPercentage;
                progress.TimeSpent = TimeSpan.FromMinutes(request.TimeSpentMinutes ?? 0);

                if (request.IsCompleted && !progress.CompletedAt.HasValue)
                {
                    progress.CompletedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return progress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating module progress for enrollment: {EnrollmentId}, module: {ModuleId}", request.EnrollmentId, request.ModuleId);
            throw;
        }
    }

    public async Task<LessonProgress> UpdateLessonProgressAsync(UpdateProgressRequest request)
    {
        try
        {
            // First, ensure module progress exists
            var moduleProgress = await GetModuleProgressAsync(request.EnrollmentId, request.ModuleId ?? 0);
            if (moduleProgress == null)
            {
                // Create module progress if it doesn't exist
                var updateModuleRequest = new UpdateProgressRequest
                {
                    EnrollmentId = request.EnrollmentId,
                    ModuleId = request.ModuleId,
                    ProgressPercentage = 0,
                    TimeSpentMinutes = 0,
                    IsCompleted = false
                };
                moduleProgress = await UpdateModuleProgressAsync(updateModuleRequest);
            }

            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.ModuleProgressId == moduleProgress.Id && lp.LessonId == request.LessonId);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    ModuleProgressId = moduleProgress.Id,
                    LessonId = request.LessonId ?? 0,
                    StartedAt = DateTime.UtcNow,
                    TimeSpent = TimeSpan.FromMinutes(request.TimeSpentMinutes ?? 0),
                    ProgressPercentage = request.IsCompleted ? 100 : request.ProgressPercentage
                };

                if (request.IsCompleted)
                {
                    progress.CompletedAt = DateTime.UtcNow;
                }

                _context.LessonProgresses.Add(progress);
            }
            else
            {
                progress.TimeSpent = TimeSpan.FromMinutes(request.TimeSpentMinutes ?? 0);
                progress.ProgressPercentage = request.IsCompleted ? 100 : request.ProgressPercentage;

                if (request.IsCompleted && !progress.CompletedAt.HasValue)
                {
                    progress.CompletedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return progress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lesson progress for enrollment: {EnrollmentId}, lesson: {LessonId}", request.EnrollmentId, request.LessonId);
            throw;
        }
    }

    public async Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(string userId)
    {
        try
        {
            return await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Include(ua => ua.Achievement)
                .OrderByDescending(ua => ua.EarnedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user achievements for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserAchievement> AddAchievementAsync(string userId, string type, string title, string description, int points = 0)
    {
        try
        {
            // Check if user already has this achievement type
            var existing = await _context.UserAchievements
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.Achievement.Name == title);
            if (existing != null)
                return existing;

            // Find or create achievement
            var achievement = await _context.Achievements
                .FirstOrDefaultAsync(a => a.Name == title);
            if (achievement == null)
            {
                achievement = new Achievement
                {
                    Name = title,
                    Description = description,
                    Points = points,
                    Type = GetAchievementType(type)
                };
                _context.Achievements.Add(achievement);
                await _context.SaveChangesAsync();
            }

            var userAchievement = new UserAchievement
            {
                UserId = userId,
                AchievementId = achievement.Id,
                EarnedAt = DateTime.UtcNow
            };

            _context.UserAchievements.Add(userAchievement);
            await _context.SaveChangesAsync();

            return userAchievement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding achievement for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int limit = 10)
    {
        try
        {
            return await _context.LeaderboardEntries
                .OrderBy(le => le.Rank)
                .Take(limit)
                .Include(le => le.User)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard");
            throw;
        }
    }

    public async Task<LeaderboardEntry?> GetUserLeaderboardPositionAsync(string userId)
    {
        try
        {
            return await _context.LeaderboardEntries
                .FirstOrDefaultAsync(le => le.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard position for user: {UserId}", userId);
            throw;
        }
    }

    public async Task UpdateLeaderboardAsync()
    {
        try
        {
            // Get user achievements and calculate total points
            var userPoints = await _context.UserAchievements
                .Include(ua => ua.Achievement)
                .GroupBy(ua => ua.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalPoints = g.Sum(ua => ua.Achievement.Points),
                    AchievementCount = g.Count()
                })
                .ToListAsync();

            // Get course completion data
            var courseCompletions = await _context.Enrollments
                .Where(e => e.CompletedAt.HasValue)
                .GroupBy(e => e.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    CoursesCompleted = g.Count()
                })
                .ToListAsync();

            // Get or create a default leaderboard
            var defaultLeaderboard = await _context.Leaderboards
                .FirstOrDefaultAsync(l => l.Type == LeaderboardType.Points && l.Period == LeaderboardPeriod.AllTime);

            if (defaultLeaderboard == null)
            {
                defaultLeaderboard = new Leaderboard
                {
                    Name = "All-Time Points Leaderboard",
                    Type = LeaderboardType.Points,
                    Period = LeaderboardPeriod.AllTime,
                    IsActive = true
                };
                _context.Leaderboards.Add(defaultLeaderboard);
                await _context.SaveChangesAsync();
            }

            // Clear existing leaderboard entries for this leaderboard
            var existingEntries = await _context.LeaderboardEntries
                .Where(le => le.LeaderboardId == defaultLeaderboard.Id)
                .ToListAsync();
            _context.LeaderboardEntries.RemoveRange(existingEntries);

            // Create new leaderboard entries
            var leaderboardData = userPoints
                .Select(up => new
                {
                    up.UserId,
                    up.TotalPoints,
                    up.AchievementCount,
                    CoursesCompleted = courseCompletions.FirstOrDefault(cc => cc.UserId == up.UserId)?.CoursesCompleted ?? 0
                })
                .OrderByDescending(x => x.TotalPoints)
                .ThenByDescending(x => x.CoursesCompleted)
                .Select((x, index) => new LeaderboardEntry
                {
                    LeaderboardId = defaultLeaderboard.Id,
                    UserId = x.UserId,
                    Rank = index + 1,
                    Score = x.TotalPoints,
                    LastUpdated = DateTime.UtcNow
                })
                .ToList();

            _context.LeaderboardEntries.AddRange(leaderboardData);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leaderboard");
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetProgressSummaryAsync(int enrollmentId)
    {
        try
        {
            var moduleProgress = await GetModuleProgressAsync(enrollmentId);
            var lessonProgress = await GetLessonProgressAsync(enrollmentId);

            var totalModules = moduleProgress.Count();
            var completedModules = moduleProgress.Count(mp => mp.IsCompleted);
            var totalLessons = lessonProgress.Count();
            var completedLessons = lessonProgress.Count(lp => lp.IsCompleted);

            var overallProgress = totalLessons > 0 ? (double)completedLessons / totalLessons * 100 : 0;
            var totalTimeSpent = lessonProgress.Sum(lp => lp.TimeSpent.TotalMinutes);

            return new Dictionary<string, object>
            {
                ["TotalModules"] = totalModules,
                ["CompletedModules"] = completedModules,
                ["TotalLessons"] = totalLessons,
                ["CompletedLessons"] = completedLessons,
                ["OverallProgress"] = Math.Round(overallProgress, 1),
                ["TotalTimeSpentMinutes"] = totalTimeSpent,
                ["TotalTimeSpentHours"] = Math.Round(totalTimeSpent / 60.0, 1)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress summary for enrollment: {EnrollmentId}", enrollmentId);
            throw;
        }
    }

    private static AchievementType GetAchievementType(string type) => type switch
    {
        "CourseCompletion" => AchievementType.Course,
        "PerfectScore" => AchievementType.Assessment,
        "FirstLesson" => AchievementType.Course,
        "Consistent" => AchievementType.Course,
        "QuickLearner" => AchievementType.Course,
        _ => AchievementType.Course
    };
}
