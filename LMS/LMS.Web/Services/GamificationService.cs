using LMS.Data.Entities;
using LMS.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Web.Services
{
    public interface IGamificationService
    {
        Task CheckAndAwardAchievements(string userId, int? courseId = null, int? assessmentId = null, double? score = null);
        Task UpdateLeaderboard(string userId, int pointsEarned);
        Task<List<Achievement>> GetAvailableAchievements(int? courseId = null);
        Task<List<UserAchievement>> GetUserAchievements(string userId);
    }

    public class GamificationService : IGamificationService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<GamificationService> _logger;

        public GamificationService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<GamificationService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task CheckAndAwardAchievements(string userId, int? courseId = null, int? assessmentId = null, double? score = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // Get all achievement criteria that could be met
                var query = context.AchievementCriteria
                    .Include(ac => ac.Achievement)
                    .Where(ac => ac.Achievement.IsActive);

                // Filter by course if specified
                if (courseId.HasValue)
                {
                    query = query.Where(ac => ac.CourseId == null || ac.CourseId == courseId.Value);
                }

                // Filter by assessment if specified
                if (assessmentId.HasValue)
                {
                    query = query.Where(ac => ac.AssessmentId == null || ac.AssessmentId == assessmentId.Value);
                }

                var applicableCriteria = await query.ToListAsync();

                foreach (var criteria in applicableCriteria)
                {
                    // Check if user already has this achievement
                    var existingAchievement = await context.UserAchievements
                        .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == criteria.AchievementId);

                    if (existingAchievement)
                        continue;

                    // Evaluate the criteria
                    bool criteriaIsMet = await EvaluateCriteria(context, criteria, userId, courseId, assessmentId, score);

                    if (criteriaIsMet)
                    {
                        await AwardAchievement(context, userId, criteria.Achievement, courseId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking achievements for user {UserId}", userId);
            }
        }

        private async Task<bool> EvaluateCriteria(ApplicationDbContext context, AchievementCriteria criteria, string userId, int? courseId, int? assessmentId, double? score)
        {
            switch (criteria.Type)
            {
                case CriteriaType.CourseCompletion:
                    return await CheckCourseCompletion(context, userId, criteria.CourseId ?? courseId);

                case CriteriaType.AssessmentScore:
                    return await CheckAssessmentScore(userId, criteria.AssessmentId ?? assessmentId, criteria.MinScore ?? 0, score);

                case CriteriaType.Participation:
                    return await CheckParticipation(context, userId, criteria.RequiredCount ?? 1, criteria.CourseId);

                case CriteriaType.TimeSpent:
                    return await CheckTimeSpent(context, userId, criteria.RequiredCount ?? 1, criteria.CourseId);

                case CriteriaType.Streak:
                    return await CheckStreak(context, userId, criteria.RequiredCount ?? 1);

                case CriteriaType.Social:
                    return await CheckSocialActivity(context, userId, criteria.RequiredCount ?? 1);

                default:
                    return false;
            }
        }

        private async Task<bool> CheckCourseCompletion(ApplicationDbContext context, string userId, int? courseId)
        {
            if (!courseId.HasValue)
                return false;

            var enrollment = await context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId.Value);

            return enrollment != null && enrollment.CompletedAt.HasValue;
        }

        private async Task<bool> CheckAssessmentScore(string userId, int? assessmentId, double minScore, double? actualScore)
        {
            if (!assessmentId.HasValue || !actualScore.HasValue)
                return false;

            return actualScore.Value >= minScore;
        }

        private async Task<bool> CheckParticipation(ApplicationDbContext context, string userId, int requiredCount, int? courseId)
        {
            var query = context.UserActivities
                .Where(ua => ua.UserId == userId &&
                            (ua.Type == ActivityType.LessonView ||
                             ua.Type == ActivityType.AssessmentAttempt ||
                             ua.Type == ActivityType.ForumPost));

            if (courseId.HasValue)
            {
                // This would need additional logic to filter by course
                // For now, we'll check total participation
            }

            var count = await query.CountAsync();
            return count >= requiredCount;
        }

        private async Task<bool> CheckTimeSpent(ApplicationDbContext context, string userId, int requiredHours, int? courseId)
        {
            // This would require tracking time spent in lessons
            // For now, we'll use a simple approximation based on lesson completions
            var lessonCount = await context.LessonProgresses
                .Where(lp => lp.ModuleProgress.Enrollment.UserId == userId && lp.CompletedAt.HasValue)
                .CountAsync();

            // Assume 30 minutes per lesson on average
            var estimatedHours = lessonCount * 0.5;
            return estimatedHours >= requiredHours;
        }

        private async Task<bool> CheckStreak(ApplicationDbContext context, string userId, int requiredDays)
        {
            // Check for consecutive days of activity
            var activities = await context.UserActivities
                .Where(ua => ua.UserId == userId)
                .OrderByDescending(ua => ua.Timestamp)
                .Take(requiredDays * 2) // Get more than needed to check streak
                .Select(ua => ua.Timestamp.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();

            if (activities.Count < requiredDays)
                return false;

            // Check for consecutive days
            for (int i = 0; i < requiredDays - 1; i++)
            {
                if (activities[i].AddDays(-1) != activities[i + 1])
                    return false;
            }

            return true;
        }

        private async Task<bool> CheckSocialActivity(ApplicationDbContext context, string userId, int requiredCount)
        {
            var socialActivities = await context.UserActivities
                .Where(ua => ua.UserId == userId &&
                            (ua.Type == ActivityType.ForumPost || ua.Type == ActivityType.MessageSent))
                .CountAsync();

            return socialActivities >= requiredCount;
        }

        private async Task AwardAchievement(ApplicationDbContext context, string userId, Achievement achievement, int? courseId)
        {
            // Create user achievement record
            var userAchievement = new UserAchievement
            {
                UserId = userId,
                AchievementId = achievement.Id,
                CourseId = courseId
            };

            context.UserAchievements.Add(userAchievement);

            // Update user's total points and level
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.TotalPoints += achievement.Points;

                // Simple level calculation: every 1000 points = 1 level
                var newLevel = (user.TotalPoints / 1000) + 1;
                if (newLevel > user.Level)
                {
                    user.Level = newLevel;
                }
            }

            await context.SaveChangesAsync();

            _logger.LogInformation("Achievement {AchievementName} awarded to user {UserId}",
                achievement.Name, userId);

            // Here you could trigger real-time notifications via SignalR
            // await _hubContext.Clients.User(userId).SendAsync("AchievementEarned", achievement);
        }

        public async Task UpdateLeaderboard(string userId, int pointsEarned)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // This is a simplified implementation
                // In a real system, you'd use a message queue for this

                var user = await context.Users.FindAsync(userId);
                if (user == null) return;

                // Find or create global leaderboard
                var globalLeaderboard = await context.Leaderboards
                    .FirstOrDefaultAsync(l => l.Type == LeaderboardType.Points &&
                                             l.Period == LeaderboardPeriod.AllTime &&
                                             l.CourseId == null);

                if (globalLeaderboard == null)
                {
                    globalLeaderboard = new Leaderboard
                    {
                        Name = "Global Points Leaderboard",
                        Type = LeaderboardType.Points,
                        Period = LeaderboardPeriod.AllTime
                    };
                    context.Leaderboards.Add(globalLeaderboard);
                    await context.SaveChangesAsync();
                }

                // Update or create leaderboard entry
                var entry = await context.LeaderboardEntries
                    .FirstOrDefaultAsync(le => le.LeaderboardId == globalLeaderboard.Id &&
                                              le.UserId == userId);

                if (entry == null)
                {
                    entry = new LeaderboardEntry
                    {
                        LeaderboardId = globalLeaderboard.Id,
                        UserId = userId,
                        Score = user.TotalPoints,
                        Rank = 1
                    };
                    context.LeaderboardEntries.Add(entry);
                }
                else
                {
                    entry.Score = user.TotalPoints;
                    entry.LastUpdated = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();

                // Recalculate ranks (simplified - in production use more efficient algorithm)
                await RecalculateLeaderboardRanks(context, globalLeaderboard.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating leaderboard for user {UserId}", userId);
            }
        }

        private async Task RecalculateLeaderboardRanks(ApplicationDbContext context, int leaderboardId)
        {
            var entries = await context.LeaderboardEntries
                .Where(le => le.LeaderboardId == leaderboardId)
                .OrderByDescending(le => le.Score)
                .ToListAsync();

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Rank = i + 1;
            }

            await context.SaveChangesAsync();
        }

        public async Task<List<Achievement>> GetAvailableAchievements(int? courseId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Achievements
                .Include(a => a.Criteria)
                .Where(a => a.IsActive);

            if (courseId.HasValue)
            {
                query = query.Where(a => a.Criteria.Any(c => c.CourseId == null || c.CourseId == courseId.Value));
            }

            return await query.ToListAsync();
        }

        public async Task<List<UserAchievement>> GetUserAchievements(string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.UserAchievements
                .Include(ua => ua.Achievement)
                .Include(ua => ua.Course)
                .Where(ua => ua.UserId == userId)
                .OrderByDescending(ua => ua.EarnedAt)
                .ToListAsync();
        }
    }
}
