using LMS.Data.Entities;
using LMS.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LMS.Web.Infrastructure
{
    public class GamificationInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GamificationInterceptor> _logger;
        private readonly List<ProgressChangeContext> _progressChanges = new();

        public GamificationInterceptor(IServiceProvider serviceProvider, ILogger<GamificationInterceptor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
            {
                CaptureProgressChanges(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            // Process gamification logic after successful save
            if (_progressChanges.Any())
            {
                await ProcessGamificationLogic();
                _progressChanges.Clear();
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void CaptureProgressChanges(DbContext context)
        {
            var progressEntries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is LessonProgress ||
                           e.Entity is ModuleProgress ||
                           e.Entity is AssessmentAttempt ||
                           e.Entity is Enrollment)
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in progressEntries)
            {
                var progressContext = CreateProgressContext(entry);
                if (progressContext != null)
                {
                    _progressChanges.Add(progressContext);
                }
            }
        }

        private ProgressChangeContext? CreateProgressContext(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            switch (entry.Entity)
            {
                case LessonProgress lessonProgress:
                    // Check if lesson was just completed
                    if (entry.State == EntityState.Modified &&
                        entry.Property(nameof(LessonProgress.CompletedAt)).IsModified &&
                        lessonProgress.CompletedAt.HasValue)
                    {
                        return new ProgressChangeContext
                        {
                            Type = ProgressChangeType.LessonCompleted,
                            UserId = GetUserIdFromLessonProgress(lessonProgress),
                            LessonId = lessonProgress.LessonId,
                            ModuleId = lessonProgress.ModuleProgress?.ModuleId,
                            CourseId = GetCourseIdFromLessonProgress(lessonProgress)
                        };
                    }
                    break;

                case ModuleProgress moduleProgress:
                    // Check if module was just completed
                    if (entry.State == EntityState.Modified &&
                        entry.Property(nameof(ModuleProgress.CompletedAt)).IsModified &&
                        moduleProgress.CompletedAt.HasValue)
                    {
                        return new ProgressChangeContext
                        {
                            Type = ProgressChangeType.ModuleCompleted,
                            UserId = moduleProgress.Enrollment.UserId,
                            ModuleId = moduleProgress.ModuleId,
                            CourseId = moduleProgress.Enrollment.CourseId
                        };
                    }
                    break;

                case AssessmentAttempt attempt:
                    // Check if assessment was just completed
                    if (entry.State == EntityState.Modified &&
                        entry.Property(nameof(AssessmentAttempt.CompletedAt)).IsModified &&
                        attempt.CompletedAt.HasValue)
                    {
                        return new ProgressChangeContext
                        {
                            Type = ProgressChangeType.AssessmentCompleted,
                            UserId = attempt.Enrollment.UserId,
                            AssessmentId = attempt.AssessmentId,
                            CourseId = attempt.Enrollment.CourseId,
                            Score = attempt.Score,
                            IsPassed = attempt.IsPassed
                        };
                    }
                    break;

                case Enrollment enrollment:
                    // Check if course was just completed
                    if (entry.State == EntityState.Modified &&
                        entry.Property(nameof(Enrollment.CompletedAt)).IsModified &&
                        enrollment.CompletedAt.HasValue)
                    {
                        return new ProgressChangeContext
                        {
                            Type = ProgressChangeType.CourseCompleted,
                            UserId = enrollment.UserId,
                            CourseId = enrollment.CourseId
                        };
                    }
                    break;
            }

            return null;
        }

        private string GetUserIdFromLessonProgress(LessonProgress lessonProgress)
        {
            // This assumes the ModuleProgress is loaded
            return lessonProgress.ModuleProgress?.Enrollment?.UserId ?? string.Empty;
        }

        private int? GetCourseIdFromLessonProgress(LessonProgress lessonProgress)
        {
            // This assumes the ModuleProgress and Enrollment are loaded
            return lessonProgress.ModuleProgress?.Enrollment?.CourseId;
        }

        private async Task ProcessGamificationLogic()
        {
            using var scope = _serviceProvider.CreateScope();
            var gamificationService = scope.ServiceProvider.GetRequiredService<IGamificationService>();

            foreach (var change in _progressChanges)
            {
                try
                {
                    await ProcessSingleChange(gamificationService, change);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing gamification for change: {ChangeType}, User: {UserId}",
                        change.Type, change.UserId);
                }
            }
        }

        private async Task ProcessSingleChange(IGamificationService gamificationService, ProgressChangeContext change)
        {
            switch (change.Type)
            {
                case ProgressChangeType.LessonCompleted:
                    await gamificationService.CheckAndAwardAchievements(
                        change.UserId,
                        change.CourseId);
                    break;

                case ProgressChangeType.ModuleCompleted:
                    await gamificationService.CheckAndAwardAchievements(
                        change.UserId,
                        change.CourseId);
                    break;

                case ProgressChangeType.AssessmentCompleted:
                    await gamificationService.CheckAndAwardAchievements(
                        change.UserId,
                        change.CourseId,
                        change.AssessmentId,
                        change.Score);
                    break;

                case ProgressChangeType.CourseCompleted:
                    await gamificationService.CheckAndAwardAchievements(
                        change.UserId,
                        change.CourseId);
                    break;
            }

            // Update leaderboard if points were earned
            // This is simplified - in production you'd track the actual points earned
            await gamificationService.UpdateLeaderboard(change.UserId, 0);
        }
    }

    public class ProgressChangeContext
    {
        public ProgressChangeType Type { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? CourseId { get; set; }
        public int? ModuleId { get; set; }
        public int? LessonId { get; set; }
        public int? AssessmentId { get; set; }
        public double? Score { get; set; }
        public bool IsPassed { get; set; }
    }

    public enum ProgressChangeType
    {
        LessonCompleted,
        ModuleCompleted,
        AssessmentCompleted,
        CourseCompleted
    }
}
