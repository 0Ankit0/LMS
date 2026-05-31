using LMS.Data.DTOs;
using LMS.Data.DTOs.LMS.Note;
using LMS.Data.Entities;
using LMS.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Web.Repositories
{
    public interface IDashboardRepository
    {
        Task<DashboardDataModel> GetDashboardDataAsync(string userId);
        Task<UserProgressSummary> GetUserProgressSummaryAsync(string userId);
        Task<List<CourseModel>> GetRecentCoursesAsync(string userId, int limit = 5);
        Task<List<CourseModel>> GetNewCoursesAsync(int limit = 6);
        Task<List<CourseModel>> GetUserCoursesAsync(string userId);
        Task<List<CourseModel>> GetUpcomingCoursesAsync(string userId);
        Task<List<AnnouncementModel>> GetRecentAnnouncementsAsync(int limit = 5);
        Task<List<ProgressCourseModel>> GetProgressCoursesAsync(string userId);
        Task<CourseModel?> GetContinueCourseAsync(string userId);
        Task<List<NoteQuickAccessDTO>> GetUserNotesAsync(string userId, int limit = 5);
    }

    public class DashboardRepository : IDashboardRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<DashboardRepository> _logger;

        public DashboardRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<DashboardRepository> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<DashboardDataModel> GetDashboardDataAsync(string userId)
        {
            try
            {
                var data = new DashboardDataModel
                {
                    ContinueCourse = await GetContinueCourseAsync(userId),
                    ProgressCourses = await GetProgressCoursesAsync(userId),
                    NewCourses = await GetNewCoursesAsync(6),
                    YourCourses = await GetUserCoursesAsync(userId),
                    UpcomingCourses = await GetUpcomingCoursesAsync(userId),
                    UserNotes = await GetUserNotesAsync(userId, 5),
                    RecentAnnouncements = await GetRecentAnnouncementsAsync(5),
                    ProgressSummary = await GetUserProgressSummaryAsync(userId)
                };

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data for user {UserId}", userId);
                return new DashboardDataModel();
            }
        }

        public async Task<UserProgressSummary> GetUserProgressSummaryAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var enrollments = await context.Enrollments
                    .Include(e => e.Course)
                    .Include(e => e.ModuleProgresses)
                    .Where(e => e.UserId == userId && e.Course.IsActive)
                    .ToListAsync();

                var totalCourses = enrollments.Count;
                var completedCourses = enrollments.Count(e => e.Status == EnrollmentStatus.Completed);
                var activeCourses = enrollments.Count(e => e.Status == EnrollmentStatus.Active);
                var overallProgress = totalCourses > 0 ?
                    enrollments.Average(e => e.ProgressPercentage) : 0;

                var totalStudyHours = enrollments
                    .Sum(e => e.TimeSpent.TotalHours);

                var assessmentAttempts = await context.AssessmentAttempts
                    .Where(aa => aa.Enrollment.UserId == userId)
                    .ToListAsync();

                var completedAssessments = assessmentAttempts.Count(a => a.CompletedAt.HasValue);
                var averageScore = assessmentAttempts.Any(a => a.Score.HasValue) ?
                    assessmentAttempts.Where(a => a.Score.HasValue).Average(a => a.Score!.Value) : 0;

                var lastActivity = enrollments
                    .SelectMany(e => e.ModuleProgresses)
                    .Where(mp => mp.CompletedAt.HasValue)
                    .OrderByDescending(mp => mp.CompletedAt)
                    .FirstOrDefault()?.CompletedAt ?? DateTime.MinValue;

                // Calculate streak (simplified)
                var recentDays = await context.ModuleProgresses
                    .Where(mp => mp.Enrollment.UserId == userId &&
                                mp.CompletedAt.HasValue &&
                                mp.CompletedAt >= DateTime.UtcNow.AddDays(-30))
                    .Select(mp => mp.CompletedAt!.Value.Date)
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToListAsync();

                var currentStreak = CalculateStreak(recentDays);

                return new UserProgressSummary
                {
                    OverallProgress = overallProgress,
                    CompletedCourses = completedCourses,
                    TotalCourses = totalCourses,
                    TotalStudyHours = (int)totalStudyHours,
                    CompletedAssessments = completedAssessments,
                    TotalAssessments = assessmentAttempts.Count,
                    ActiveCourses = activeCourses,
                    LastActivity = lastActivity,
                    AverageScore = averageScore,
                    CurrentStreak = currentStreak,
                    ActiveDays = recentDays.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user progress summary for user {UserId}", userId);
                return new UserProgressSummary();
            }
        }

        public async Task<List<CourseModel>> GetRecentCoursesAsync(string userId, int limit = 5)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var recentCourses = await context.Enrollments
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Instructor)
                    .Include(e => e.Course)
                        .ThenInclude(c => c.CourseCategories)
                            .ThenInclude(cc => cc.Category)
                    .Include(e => e.ModuleProgresses)
                    .Where(e => e.UserId == userId && e.Course.IsActive)
                    .OrderByDescending(e => e.ModuleProgresses.Any() ?
                                           e.ModuleProgresses.Max(mp => mp.CompletedAt ?? mp.StartedAt) :
                                           e.EnrolledAt)
                    .Take(limit)
                    .Select(e => e.Course)
                    .ToListAsync();

                return recentCourses.Select(MapToCourseModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent courses for user {UserId}", userId);
                return new List<CourseModel>();
            }
        }

        public async Task<List<CourseModel>> GetNewCoursesAsync(int limit = 6)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var newCourses = await context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.CourseCategories)
                        .ThenInclude(cc => cc.Category)
                    .Where(c => c.IsActive && c.Status == CourseStatus.Published)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(limit)
                    .ToListAsync();

                return newCourses.Select(MapToCourseModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting new courses");
                return new List<CourseModel>();
            }
        }

        public async Task<List<CourseModel>> GetUserCoursesAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var userCourses = await context.Enrollments
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Instructor)
                    .Include(e => e.Course)
                        .ThenInclude(c => c.CourseCategories)
                            .ThenInclude(cc => cc.Category)
                    .Where(e => e.UserId == userId && e.Course.IsActive)
                    .Select(e => e.Course)
                    .ToListAsync();

                return userCourses.Select(MapToCourseModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user courses for user {UserId}", userId);
                return new List<CourseModel>();
            }
        }

        public async Task<List<CourseModel>> GetUpcomingCoursesAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var upcomingCourses = await context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.CourseCategories)
                        .ThenInclude(cc => cc.Category)
                    .Where(c => c.IsActive &&
                               c.Status == CourseStatus.Published &&
                               c.StartDate > DateTime.UtcNow)
                    .OrderBy(c => c.StartDate)
                    .Take(10)
                    .ToListAsync();

                return upcomingCourses.Select(MapToCourseModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming courses for user {UserId}", userId);
                return new List<CourseModel>();
            }
        }

        public async Task<List<AnnouncementModel>> GetRecentAnnouncementsAsync(int limit = 5)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var announcements = await context.Announcements
                    .Include(a => a.Author)
                    .Include(a => a.Course)
                    .Where(a => a.IsActive &&
                               (!a.ExpiresAt.HasValue || a.ExpiresAt > DateTime.UtcNow))
                    .OrderByDescending(a => a.PublishedAt)
                    .Take(limit)
                    .ToListAsync();

                return announcements.Select(MapToAnnouncementModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent announcements");
                return new List<AnnouncementModel>();
            }
        }

        public async Task<List<ProgressCourseModel>> GetProgressCoursesAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var progressCourses = await context.Enrollments
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Instructor)
                    .Include(e => e.Course)
                        .ThenInclude(c => c.CourseCategories)
                            .ThenInclude(cc => cc.Category)
                    .Include(e => e.ModuleProgresses)
                    .Where(e => e.UserId == userId &&
                               e.Course.IsActive &&
                               e.ProgressPercentage > 0 &&
                               e.ProgressPercentage < 100)
                    .ToListAsync();

                return progressCourses.Select(e => new ProgressCourseModel
                {
                    Course = MapToCourseModel(e.Course),
                    Progress = e.ProgressPercentage,
                    LastAccessedAt = e.ModuleProgresses.Any() ?
                                    e.ModuleProgresses.Max(mp => mp.CompletedAt ?? mp.StartedAt ?? e.EnrolledAt) :
                                    e.EnrolledAt,
                    TimeSpent = e.TimeSpent
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress courses for user {UserId}", userId);
                return new List<ProgressCourseModel>();
            }
        }

        public async Task<CourseModel?> GetContinueCourseAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var lastAccessedCourse = await context.Enrollments
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Instructor)
                    .Include(e => e.Course)
                        .ThenInclude(c => c.CourseCategories)
                            .ThenInclude(cc => cc.Category)
                    .Include(e => e.ModuleProgresses)
                    .Where(e => e.UserId == userId &&
                               e.Course.IsActive &&
                               e.ProgressPercentage > 0 &&
                               e.ProgressPercentage < 100)
                    .OrderByDescending(e => e.ModuleProgresses.Any() ?
                                           e.ModuleProgresses.Max(mp => mp.CompletedAt ?? mp.StartedAt) :
                                           e.EnrolledAt)
                    .FirstOrDefaultAsync();

                return lastAccessedCourse != null ? MapToCourseModel(lastAccessedCourse.Course) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting continue course for user {UserId}", userId);
                return null;
            }
        }

        public async Task<List<NoteQuickAccessDTO>> GetUserNotesAsync(string userId, int limit = 5)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var notes = await context.Notes
                    .Include(n => n.Course)
                    .Include(n => n.Lesson)
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.UpdatedAt)
                    .Take(limit)
                    .ToListAsync();

                return notes.Select(n => new NoteQuickAccessDTO
                {
                    Id = n.Id,
                    Title = n.Title,
                    CourseTitle = n.Course?.Title,
                    Type = n.Type,
                    Priority = n.Priority,
                    IsPinned = n.IsPinned,
                    CreatedAt = n.CreatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notes for user {UserId}", userId);
                return new List<NoteQuickAccessDTO>();
            }
        }

        private CourseModel MapToCourseModel(Course course)
        {
            return new CourseModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                ThumbnailUrl = course.ThumbnailUrl ?? "/images/default-course.png",
                InstructorName = course.Instructor?.FullName ?? "Unknown",
                InstructorId = course.InstructorId,
                Level = course.Level.ToString(),
                Status = course.Status.ToString(),
                MaxEnrollments = course.MaxEnrollments,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                EstimatedDuration = course.EstimatedDuration,
                Prerequisites = course.Prerequisites,
                LearningObjectives = course.LearningObjectives,
                EnrollmentCount = course.Enrollments?.Count ?? 0,
                AverageRating = 0, // TODO: Calculate from ratings
                Categories = course.CourseCategories?.Select(cc => cc.Category.Name).ToList() ?? new List<string>(),
                Tags = new List<string>(), // TODO: Implement tags
                Modules = new List<ModuleModel>(), // Not needed for dashboard
                Assessments = new List<AssessmentModel>() // Not needed for dashboard
            };
        }

        private AnnouncementModel MapToAnnouncementModel(Announcement announcement)
        {
            return new AnnouncementModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                AuthorId = announcement.AuthorId,
                AuthorName = announcement.Author?.FullName ?? "System",
                Type = MapAnnouncementType(announcement.Type),
                IsPinned = false, // TODO: Add to entity
                CourseId = announcement.CourseId,
                CourseName = announcement.Course?.Title,
                CourseTitle = announcement.Course?.Title,
                Priority = announcement.Priority.ToString(),
                CreatedAt = announcement.PublishedAt,
                UpdatedAt = announcement.PublishedAt,
                PublishedAt = announcement.PublishedAt,
                ExpiresAt = announcement.ExpiresAt,
                IsActive = announcement.IsActive,
                SendEmail = announcement.SendEmail,
                SendSms = announcement.SendSms
            };
        }

        private LMS.Data.DTOs.AnnouncementType MapAnnouncementType(LMS.Data.Entities.AnnouncementType type)
        {
            return type switch
            {
                LMS.Data.Entities.AnnouncementType.General => LMS.Data.DTOs.AnnouncementType.General,
                LMS.Data.Entities.AnnouncementType.Event => LMS.Data.DTOs.AnnouncementType.Course,
                LMS.Data.Entities.AnnouncementType.Deadline => LMS.Data.DTOs.AnnouncementType.System,
                LMS.Data.Entities.AnnouncementType.Alert => LMS.Data.DTOs.AnnouncementType.Emergency,
                _ => LMS.Data.DTOs.AnnouncementType.General
            };
        }

        private int CalculateStreak(List<DateTime> activeDays)
        {
            if (!activeDays.Any()) return 0;

            var streak = 0;
            var currentDate = DateTime.UtcNow.Date;

            foreach (var day in activeDays)
            {
                if (day == currentDate.AddDays(-streak))
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }
    }
}