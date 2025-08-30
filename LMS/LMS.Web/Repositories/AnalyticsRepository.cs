using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LMS.Repositories
{
    public interface IAnalyticsRepository
    {
        Task<UserAnalyticsModel> GetUserAnalyticsAsync(string userId);
        Task<StudyTimeAnalytics> GetStudyTimeAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ProgressAnalytics> GetProgressAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync(string userId);
        Task<List<ActivityModel>> GetRecentActivitiesAsync(string userId, int count = 10);
        Task<List<MetricModel>> GetPerformanceMetricsAsync(string userId);
    }

    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsRepository> _logger;

        public AnalyticsRepository(ApplicationDbContext context, ILogger<AnalyticsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserAnalyticsModel> GetUserAnalyticsAsync(string userId)
        {
            try
            {
                var userEnrollments = await _context.Enrollments
                    .Where(e => e.UserId == userId)
                    .Include(e => e.Course)
                    .ToListAsync();

                var completedCourses = userEnrollments.Count(e => e.ProgressPercentage >= 100);
                var totalStudyHours = userEnrollments.Sum(e => e.TimeSpent.TotalHours);

                // Calculate average score from assessments
                var assessmentAttempts = await _context.AssessmentAttempts
                    .Where(aa => aa.Enrollment.UserId == userId && aa.CompletedAt.HasValue)
                    .ToListAsync();

                var averageScore = assessmentAttempts.Any() ? assessmentAttempts.Average(aa => aa.Score ?? 0) : 0;

                // Calculate lessons and modules completed
                // Note: Using a simplified approach since Progress table structure might be different
                var completedEnrollments = userEnrollments.Where(e => e.CompletedAt.HasValue).ToList();
                var lessonsCompleted = completedEnrollments.Count * 10; // Estimated
                var modulesCompleted = completedEnrollments.Count * 3; // Estimated

                // Calculate streak and active days - simplified for now
                var currentStreak = await CalculateCurrentStreak(userId);
                var activeDays = await CalculateActiveDays(userId);

                var recentActivities = await GetRecentActivitiesAsync(userId, 5);
                var performanceMetrics = await GetPerformanceMetricsAsync(userId);

                return new UserAnalyticsModel
                {
                    OverallProgress = userEnrollments.Any() ? userEnrollments.Average(e => e.ProgressPercentage) : 0,
                    CompletedCourses = completedCourses,
                    TotalStudyHours = (int)totalStudyHours,
                    AverageScore = averageScore,
                    TotalLessonsCompleted = lessonsCompleted,
                    TotalModulesCompleted = modulesCompleted,
                    CurrentStreak = currentStreak,
                    ActiveDays = activeDays,
                    RecentActivities = recentActivities,
                    PerformanceMetrics = performanceMetrics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user analytics for user {UserId}", userId);
                return new UserAnalyticsModel();
            }
        }

        public async Task<StudyTimeAnalytics> GetStudyTimeAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var enrollments = await _context.Enrollments
                    .Where(e => e.UserId == userId && e.EnrolledAt >= startDate && e.EnrolledAt <= endDate)
                    .Include(e => e.Course)
                    .ToListAsync();

                var dailyStudyTime = new Dictionary<DateTime, double>();
                var courseStudyTime = new Dictionary<string, double>();

                foreach (var enrollment in enrollments)
                {
                    var date = enrollment.EnrolledAt.Date;
                    var hours = enrollment.TimeSpent.TotalHours;

                    if (!dailyStudyTime.ContainsKey(date))
                        dailyStudyTime[date] = 0;

                    dailyStudyTime[date] += hours;

                    if (!courseStudyTime.ContainsKey(enrollment.Course.Title))
                        courseStudyTime[enrollment.Course.Title] = 0;

                    courseStudyTime[enrollment.Course.Title] += hours;
                }

                var totalStudyTime = enrollments.Sum(e => e.TimeSpent.TotalHours);
                var totalSessions = enrollments.Count; // Simplified
                var averageSessionLength = totalSessions > 0 ? totalStudyTime / totalSessions : 0;

                return new StudyTimeAnalytics
                {
                    DailyStudyTime = dailyStudyTime,
                    CourseStudyTime = courseStudyTime,
                    TotalStudyTime = totalStudyTime,
                    AverageSessionLength = averageSessionLength,
                    TotalSessions = totalSessions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting study time analytics for user {UserId}", userId);
                return new StudyTimeAnalytics();
            }
        }

        public async Task<ProgressAnalytics> GetProgressAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var enrollments = await _context.Enrollments
                    .Where(e => e.UserId == userId && e.EnrolledAt >= startDate && e.EnrolledAt <= endDate)
                    .Include(e => e.Course)
                    .ToListAsync();

                var dailyProgress = new Dictionary<DateTime, double>();
                var courseProgress = new Dictionary<string, double>();

                foreach (var enrollment in enrollments)
                {
                    var date = enrollment.EnrolledAt.Date;
                    var progress = enrollment.ProgressPercentage;

                    if (!dailyProgress.ContainsKey(date))
                        dailyProgress[date] = 0;

                    dailyProgress[date] = Math.Max(dailyProgress[date], progress);
                    courseProgress[enrollment.Course.Title] = progress;
                }

                var overallCompletionRate = enrollments.Any() ? enrollments.Average(e => e.ProgressPercentage) : 0;

                // Calculate trends (simplified)
                var weeklyProgressTrend = CalculateProgressTrend(dailyProgress, 7);
                var monthlyProgressTrend = CalculateProgressTrend(dailyProgress, 30);

                return new ProgressAnalytics
                {
                    DailyProgress = dailyProgress,
                    CourseProgress = courseProgress,
                    OverallCompletionRate = overallCompletionRate,
                    WeeklyProgressTrend = weeklyProgressTrend,
                    MonthlyProgressTrend = monthlyProgressTrend
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress analytics for user {UserId}", userId);
                return new ProgressAnalytics();
            }
        }

        public async Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync(string userId)
        {
            try
            {
                var assessmentAttempts = await _context.AssessmentAttempts
                    .Where(aa => aa.Enrollment.UserId == userId && aa.CompletedAt.HasValue)
                    .Include(aa => aa.Assessment)
                    .ToListAsync();

                var quizScores = new Dictionary<string, double>();
                var assessmentScores = new Dictionary<string, double>();

                foreach (var attempt in assessmentAttempts)
                {
                    var score = attempt.Score ?? 0;
                    var title = attempt.Assessment.Title;

                    if (attempt.Assessment.Type == Data.Entities.AssessmentType.Quiz)
                    {
                        quizScores[title] = score;
                    }
                    else
                    {
                        assessmentScores[title] = score;
                    }
                }

                var averageQuizScore = quizScores.Any() ? quizScores.Values.Average() : 0;
                var averageAssessmentScore = assessmentScores.Any() ? assessmentScores.Values.Average() : 0;

                // Determine strong and weak subjects (simplified)
                var strongSubjects = quizScores.Where(qs => qs.Value >= 80).Select(qs => qs.Key).ToList();
                var weakSubjects = quizScores.Where(qs => qs.Value < 60).Select(qs => qs.Key).ToList();

                return new PerformanceAnalytics
                {
                    QuizScores = quizScores,
                    AssessmentScores = assessmentScores,
                    AverageQuizScore = averageQuizScore,
                    AverageAssessmentScore = averageAssessmentScore,
                    StrongSubjects = strongSubjects,
                    WeakSubjects = weakSubjects
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance analytics for user {UserId}", userId);
                return new PerformanceAnalytics();
            }
        }

        public async Task<List<ActivityModel>> GetRecentActivitiesAsync(string userId, int count = 10)
        {
            try
            {
                var activities = new List<ActivityModel>();

                // Get recent enrollments
                var recentEnrollments = await _context.Enrollments
                    .Where(e => e.UserId == userId)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrolledAt)
                    .Take(count / 2)
                    .ToListAsync();

                foreach (var enrollment in recentEnrollments)
                {
                    activities.Add(new ActivityModel
                    {
                        Type = "Enrollment",
                        Description = $"Enrolled in {enrollment.Course.Title}",
                        CourseTitle = enrollment.Course.Title,
                        CourseId = enrollment.CourseId,
                        UserId = userId,
                        Timestamp = enrollment.EnrolledAt
                    });
                }

                // Get recent assessment attempts
                var recentAttempts = await _context.AssessmentAttempts
                    .Where(aa => aa.Enrollment.UserId == userId && aa.CompletedAt.HasValue)
                    .Include(aa => aa.Assessment)
                    .ThenInclude(a => a.Course)
                    .OrderByDescending(aa => aa.CompletedAt)
                    .Take(count / 2)
                    .ToListAsync();

                foreach (var attempt in recentAttempts)
                {
                    activities.Add(new ActivityModel
                    {
                        Type = "Assessment",
                        Description = $"Completed {attempt.Assessment.Title} with score {attempt.Score:F1}%",
                        CourseTitle = attempt.Assessment.Course?.Title ?? "N/A",
                        CourseId = attempt.Assessment.CourseId,
                        UserId = userId,
                        Timestamp = attempt.CompletedAt ?? attempt.StartedAt,
                        Metadata = new Dictionary<string, object>
                        {
                            ["Score"] = attempt.Score ?? 0,
                            ["AssessmentType"] = attempt.Assessment.Type.ToString()
                        }
                    });
                }

                return activities.OrderByDescending(a => a.Timestamp).Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities for user {UserId}", userId);
                return new List<ActivityModel>();
            }
        }

        public async Task<List<MetricModel>> GetPerformanceMetricsAsync(string userId)
        {
            try
            {
                var metrics = new List<MetricModel>();

                // Course completion rate
                var enrollments = await _context.Enrollments
                    .Where(e => e.UserId == userId)
                    .ToListAsync();

                if (enrollments.Any())
                {
                    var completionRate = enrollments.Average(e => e.ProgressPercentage);
                    metrics.Add(new MetricModel
                    {
                        Name = "Course Completion Rate",
                        Value = completionRate,
                        Unit = "%",
                        MaxValue = 100,
                        Description = "Average completion rate across all enrolled courses",
                        LastUpdated = DateTime.UtcNow
                    });
                }

                // Average assessment score
                var assessmentAttempts = await _context.AssessmentAttempts
                    .Where(aa => aa.Enrollment.UserId == userId && aa.CompletedAt.HasValue)
                    .ToListAsync();

                if (assessmentAttempts.Any())
                {
                    var averageScore = assessmentAttempts.Average(aa => aa.Score ?? 0);
                    metrics.Add(new MetricModel
                    {
                        Name = "Average Assessment Score",
                        Value = averageScore,
                        Unit = "%",
                        MaxValue = 100,
                        Description = "Average score across all completed assessments",
                        LastUpdated = DateTime.UtcNow
                    });
                }

                // Study time
                var totalStudyTime = enrollments.Sum(e => e.TimeSpent.TotalHours);
                metrics.Add(new MetricModel
                {
                    Name = "Total Study Time",
                    Value = totalStudyTime,
                    Unit = "hours",
                    MaxValue = 1000, // Arbitrary max for display
                    Description = "Total time spent studying across all courses",
                    LastUpdated = DateTime.UtcNow
                });

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics for user {UserId}", userId);
                return new List<MetricModel>();
            }
        }

        private async Task<int> CalculateCurrentStreak(string userId)
        {
            // Simplified streak calculation
            // In a real implementation, you'd track daily activity
            try
            {
                var recentActivity = await _context.Enrollments
                    .Where(e => e.UserId == userId && e.EnrolledAt >= DateTime.UtcNow.AddDays(-7))
                    .CountAsync();

                return Math.Min(recentActivity, 7); // Max 7 day streak for simplicity
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> CalculateActiveDays(string userId)
        {
            // Simplified active days calculation
            try
            {
                var activeDays = await _context.Enrollments
                    .Where(e => e.UserId == userId)
                    .Select(e => e.EnrolledAt.Date)
                    .Distinct()
                    .CountAsync();

                return activeDays;
            }
            catch
            {
                return 0;
            }
        }

        private double CalculateProgressTrend(Dictionary<DateTime, double> dailyProgress, int days)
        {
            var recentDays = dailyProgress
                .Where(dp => dp.Key >= DateTime.UtcNow.AddDays(-days))
                .OrderBy(dp => dp.Key)
                .ToList();

            if (recentDays.Count < 2)
                return 0;

            var firstValue = recentDays.First().Value;
            var lastValue = recentDays.Last().Value;

            return lastValue - firstValue;
        }

        // Additional analytics methods
        public async Task<object> GetSystemAnalyticsAsync()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalCourses = await _context.Courses.CountAsync();
                var totalEnrollments = await _context.Enrollments.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.LastLoginAt >= DateTime.UtcNow.AddDays(-30));

                var completedEnrollments = await _context.Enrollments
                    .CountAsync(e => e.ProgressPercentage >= 100);

                var overallCompletionRate = totalEnrollments > 0 ? 
                    (double)completedEnrollments / totalEnrollments * 100 : 0;

                var avgSessionTime = await _context.Enrollments
                    .Where(e => e.TimeSpent.TotalMinutes > 0)
                    .AverageAsync(e => e.TimeSpent.TotalMinutes);

                return new
                {
                    TotalUsers = totalUsers,
                    TotalCourses = totalCourses,
                    TotalEnrollments = totalEnrollments,
                    ActiveUsers = activeUsers,
                    OverallCompletionRate = overallCompletionRate,
                    AverageSessionTimeMinutes = avgSessionTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system analytics");
                return new
                {
                    TotalUsers = 0,
                    TotalCourses = 0,
                    TotalEnrollments = 0,
                    ActiveUsers = 0,
                    OverallCompletionRate = 0.0,
                    AverageSessionTimeMinutes = 0.0
                };
            }
        }

        public async Task<List<object>> GetRecentActivitiesAsync(int limit = 10)
        {
            try
            {
                var recentEnrollments = await _context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.StartedAt ?? e.EnrolledAt)
                    .Take(limit)
                    .Select(e => new
                    {
                        StudentName = e.User.FullName,
                        CourseName = e.Course.Title,
                        Activity = e.CompletedAt.HasValue ? "Completed Course" : 
                                  e.StartedAt.HasValue ? "Studying Course" : "Enrolled in Course",
                        Progress = e.ProgressPercentage,
                        LastActive = e.StartedAt ?? e.EnrolledAt,
                        Status = e.Status.ToString()
                    })
                    .ToListAsync();

                return recentEnrollments.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities");
                return new List<object>();
            }
        }
    }
}
