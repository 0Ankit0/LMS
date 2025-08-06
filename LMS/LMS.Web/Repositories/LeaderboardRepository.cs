using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LMS.Repositories
{
    public interface ILeaderboardRepository
    {
        Task<List<LeaderboardModel>> GetLeaderboardsAsync();
        Task<LeaderboardModel?> GetLeaderboardByIdAsync(int id);
        Task<LeaderboardModel?> GetLeaderboardByCourseIdAsync(int courseId);
        Task<List<LeaderboardEntryModel>> GetLeaderboardEntriesAsync(int leaderboardId, int limit = 50);
        Task<List<LeaderboardEntryModel>> GetGlobalLeaderboardAsync(int limit = 50);
        Task<List<LeaderboardEntryModel>> GetCourseLeaderboardAsync(int courseId, int limit = 50);
        Task<List<LeaderboardEntryModel>> GetWeeklyLeaderboardAsync(int limit = 50);
        Task<List<LeaderboardEntryModel>> GetMonthlyLeaderboardAsync(int limit = 50);
        Task<List<LeaderboardEntryModel>> GetOverallLeaderboardAsync(int limit = 50);
        Task UpdateUserScoreAsync(string userId, int courseId, double score);
        Task<int?> GetUserRankAsync(string userId, int? courseId = null);
    }

    public class LeaderboardRepository : ILeaderboardRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaderboardRepository> _logger;

        public LeaderboardRepository(ApplicationDbContext context, ILogger<LeaderboardRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<LeaderboardModel>> GetLeaderboardsAsync()
        {
            try
            {
                var leaderboards = await _context.Leaderboards
                    .Include(l => l.Course)
                    .Include(l => l.Entries)
                    .ThenInclude(e => e.User)
                    .OrderBy(l => l.Name)
                    .ToListAsync();

                return leaderboards.Select(MapToLeaderboardModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leaderboards");
                throw;
            }
        }

        public async Task<LeaderboardModel?> GetLeaderboardByIdAsync(int id)
        {
            try
            {
                var leaderboard = await _context.Leaderboards
                    .Include(l => l.Course)
                    .Include(l => l.Entries.OrderByDescending(e => e.Score).Take(50))
                    .ThenInclude(e => e.User)
                    .FirstOrDefaultAsync(l => l.Id == id);

                return leaderboard != null ? MapToLeaderboardModel(leaderboard) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching leaderboard by id {id}");
                throw;
            }
        }

        public async Task<LeaderboardModel?> GetLeaderboardByCourseIdAsync(int courseId)
        {
            try
            {
                var leaderboard = await _context.Leaderboards
                    .Include(l => l.Course)
                    .Include(l => l.Entries.OrderByDescending(e => e.Score).Take(50))
                    .ThenInclude(e => e.User)
                    .FirstOrDefaultAsync(l => l.CourseId == courseId);

                return leaderboard != null ? MapToLeaderboardModel(leaderboard) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching leaderboard by course id {courseId}");
                throw;
            }
        }

        public async Task<List<LeaderboardEntryModel>> GetLeaderboardEntriesAsync(int leaderboardId, int limit = 50)
        {
            try
            {
                var entries = await _context.LeaderboardEntries
                    .Include(e => e.User)
                    .Where(e => e.LeaderboardId == leaderboardId)
                    .OrderByDescending(e => e.Score)
                    .Take(limit)
                    .ToListAsync();

                return entries.Select((entry, index) => MapToLeaderboardEntryModel(entry, index + 1)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching leaderboard entries for leaderboard id {leaderboardId}");
                throw;
            }
        }

        public async Task<List<LeaderboardEntryModel>> GetGlobalLeaderboardAsync(int limit = 50)
        {
            try
            {
                // Get global leaderboard based on total achievement points
                var userScores = await _context.UserAchievements
                    .Include(ua => ua.User)
                    .Include(ua => ua.Achievement)
                    .GroupBy(ua => ua.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        User = g.First().User,
                        TotalScore = g.Sum(ua => ua.Achievement.Points),
                        LastUpdated = g.Max(ua => ua.EarnedAt)
                    })
                    .OrderByDescending(x => x.TotalScore)
                    .Take(limit)
                    .ToListAsync();

                return userScores.Select((score, index) => new LeaderboardEntryModel
                {
                    Rank = index + 1,
                    UserId = score.UserId,
                    UserName = score.User?.UserName ?? "",
                    ProfilePictureUrl = score.User?.ProfilePictureUrl,
                    Score = score.TotalScore,
                    LastUpdated = score.LastUpdated
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching global leaderboard");
                throw;
            }
        }

        public async Task<List<LeaderboardEntryModel>> GetCourseLeaderboardAsync(int courseId, int limit = 50)
        {
            try
            {
                // Get course-specific leaderboard based on course progress and achievements
                var courseScores = await _context.Enrollments
                    .Include(e => e.User)
                    .Where(e => e.CourseId == courseId)
                    .Select(e => new
                    {
                        UserId = e.UserId,
                        User = e.User,
                        ProgressScore = e.ProgressPercentage,
                        AchievementScore = _context.UserAchievements
                            .Where(ua => ua.UserId == e.UserId && ua.CourseId == courseId)
                            .Sum(ua => ua.Achievement.Points),
                        LastUpdated = e.CompletedAt ?? DateTime.UtcNow
                    })
                    .ToListAsync();

                var rankedScores = courseScores
                    .Select(s => new
                    {
                        s.UserId,
                        s.User,
                        TotalScore = s.ProgressScore + s.AchievementScore,
                        s.LastUpdated
                    })
                    .OrderByDescending(s => s.TotalScore)
                    .Take(limit)
                    .ToList();

                return rankedScores.Select((score, index) => new LeaderboardEntryModel
                {
                    Rank = index + 1,
                    UserId = score.UserId,
                    UserName = score.User?.UserName ?? "",
                    ProfilePictureUrl = score.User?.ProfilePictureUrl,
                    Score = score.TotalScore,
                    LastUpdated = score.LastUpdated
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching course leaderboard for course id {courseId}");
                throw;
            }
        }

        public async Task UpdateUserScoreAsync(string userId, int courseId, double score)
        {
            try
            {
                var leaderboard = await _context.Leaderboards
                    .FirstOrDefaultAsync(l => l.CourseId == courseId);

                if (leaderboard == null)
                {
                    // Create leaderboard for the course if it doesn't exist
                    var course = await _context.Courses.FindAsync(courseId);
                    if (course != null)
                    {
                        leaderboard = new Leaderboard
                        {
                            Name = $"{course.Title} Leaderboard",
                            Description = $"Leaderboard for {course.Title}",
                            Type = LeaderboardType.CourseCompletion,
                            Period = LeaderboardPeriod.AllTime,
                            CourseId = courseId,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Leaderboards.Add(leaderboard);
                        await _context.SaveChangesAsync();
                    }
                }

                if (leaderboard != null)
                {
                    var entry = await _context.LeaderboardEntries
                        .FirstOrDefaultAsync(e => e.LeaderboardId == leaderboard.Id && e.UserId == userId);

                    if (entry == null)
                    {
                        entry = new LeaderboardEntry
                        {
                            LeaderboardId = leaderboard.Id,
                            UserId = userId,
                            Score = score,
                            LastUpdated = DateTime.UtcNow
                        };
                        _context.LeaderboardEntries.Add(entry);
                    }
                    else
                    {
                        entry.Score = score;
                        entry.LastUpdated = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user score for user id {userId} and course id {courseId}");
                throw;
            }
        }

        public async Task<int?> GetUserRankAsync(string userId, int? courseId = null)
        {
            try
            {
                if (courseId.HasValue)
                {
                    var courseRanking = await GetCourseLeaderboardAsync(courseId.Value, 1000);
                    var userEntry = courseRanking.FirstOrDefault(e => e.UserId == userId);
                    return userEntry?.Rank;
                }
                else
                {
                    var globalRanking = await GetGlobalLeaderboardAsync(1000);
                    var userEntry = globalRanking.FirstOrDefault(e => e.UserId == userId);
                    return userEntry?.Rank;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching user rank for user id {userId}");
                throw;
            }
        }

        public async Task<List<LeaderboardEntryModel>> GetWeeklyLeaderboardAsync(int limit = 50)
        {
            try
            {
                var weekAgo = DateTime.UtcNow.AddDays(-7);

                var entries = await _context.LeaderboardEntries
                    .Include(e => e.User)
                    .Where(e => e.LastUpdated >= weekAgo)
                    .GroupBy(e => e.UserId)
                    .Select(g => new { UserId = g.Key, TotalScore = g.Sum(e => e.Score), User = g.First().User, LastUpdated = g.Max(e => e.LastUpdated) })
                    .OrderByDescending(e => e.TotalScore)
                    .Take(limit)
                    .ToListAsync();

                return entries.Select((entry, index) => new LeaderboardEntryModel
                {
                    Rank = index + 1,
                    UserId = entry.UserId,
                    UserName = entry.User?.UserName ?? "",
                    ProfilePictureUrl = entry.User?.ProfilePictureUrl,
                    TotalPoints = (int)entry.TotalScore,
                    Score = entry.TotalScore,
                    LastUpdated = entry.LastUpdated
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weekly leaderboard");
                throw;
            }
        }

        public async Task<List<LeaderboardEntryModel>> GetMonthlyLeaderboardAsync(int limit = 50)
        {
            try
            {
                var monthAgo = DateTime.UtcNow.AddDays(-30);

                var entries = await _context.LeaderboardEntries
                    .Include(e => e.User)
                    .Where(e => e.LastUpdated >= monthAgo)
                    .GroupBy(e => e.UserId)
                    .Select(g => new { UserId = g.Key, TotalScore = g.Sum(e => e.Score), User = g.First().User, LastUpdated = g.Max(e => e.LastUpdated) })
                    .OrderByDescending(e => e.TotalScore)
                    .Take(limit)
                    .ToListAsync();

                return entries.Select((entry, index) => new LeaderboardEntryModel
                {
                    Rank = index + 1,
                    UserId = entry.UserId,
                    UserName = entry.User?.UserName ?? "",
                    ProfilePictureUrl = entry.User?.ProfilePictureUrl,
                    TotalPoints = (int)entry.TotalScore,
                    Score = entry.TotalScore,
                    LastUpdated = entry.LastUpdated
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching monthly leaderboard");
                throw;
            }
        }

        public async Task<List<LeaderboardEntryModel>> GetOverallLeaderboardAsync(int limit = 50)
        {
            try
            {
                // This is essentially the same as GetGlobalLeaderboardAsync but with additional properties
                var entries = await _context.LeaderboardEntries
                    .Include(e => e.User)
                    .GroupBy(e => e.UserId)
                    .Select(g => new { UserId = g.Key, TotalScore = g.Sum(e => e.Score), User = g.First().User, LastUpdated = g.Max(e => e.LastUpdated) })
                    .OrderByDescending(e => e.TotalScore)
                    .Take(limit)
                    .ToListAsync();

                return entries.Select((entry, index) => new LeaderboardEntryModel
                {
                    Rank = index + 1,
                    UserId = entry.UserId,
                    UserName = entry.User?.UserName ?? "",
                    ProfilePictureUrl = entry.User?.ProfilePictureUrl,
                    TotalPoints = (int)entry.TotalScore,
                    Score = entry.TotalScore,
                    LastUpdated = entry.LastUpdated
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching overall leaderboard");
                throw;
            }
        }

        private static LeaderboardModel MapToLeaderboardModel(Leaderboard leaderboard)
        {
            return new LeaderboardModel
            {
                Id = leaderboard.Id,
                Name = leaderboard.Name,
                Description = leaderboard.Description,
                Type = leaderboard.Type.ToString(),
                Period = leaderboard.Period.ToString(),
                CourseName = leaderboard.Course?.Title,
                Entries = leaderboard.Entries?.OrderByDescending(e => e.Score)
                    .Select((entry, index) => MapToLeaderboardEntryModel(entry, index + 1))
                    .ToList() ?? new List<LeaderboardEntryModel>()
            };
        }

        private static LeaderboardEntryModel MapToLeaderboardEntryModel(LeaderboardEntry entry, int rank)
        {
            return new LeaderboardEntryModel
            {
                Rank = rank,
                UserId = entry.UserId,
                UserName = entry.User?.UserName ?? "",
                ProfilePictureUrl = entry.User?.ProfilePictureUrl,
                Score = entry.Score,
                LastUpdated = entry.LastUpdated
            };
        }
    }
}
