using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories
{
    public interface IAnnouncementRepository
    {
        Task<List<AnnouncementModel>> GetAnnouncementsAsync();
        Task<List<AnnouncementModel>> GetLatestAnnouncementsAsync();
        Task<List<AnnouncementModel>> GetAnnouncementsByCourseAsync(int courseId);
        Task<AnnouncementModel?> GetAnnouncementByIdAsync(int id);
        Task<AnnouncementModel> CreateAnnouncementAsync(CreateAnnouncementRequest request);
        Task<AnnouncementModel> UpdateAnnouncementAsync(int id, CreateAnnouncementRequest request);
        Task<bool> DeleteAnnouncementAsync(int id);

        // Filtered announcements
        Task<List<AnnouncementModel>> GetFilteredAnnouncementsAsync(string? searchTerm, string? priority, string? sortBy);
        Task<List<AnnouncementModel>> GetAllAnnouncementsAsync();
    }

    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<AnnouncementRepository> _logger;

        public AnnouncementRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<AnnouncementRepository> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<AnnouncementModel>> GetAnnouncementsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                // Only get active announcements from the database
                var dbAnnouncements = await context.Announcements
                    .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow))
                    .OrderByDescending(a => a.Id)
                    .Select(a => new AnnouncementModel
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Content = a.Content,
                        AuthorId = a.AuthorId,
                        AuthorName = a.Author.FullName,
                        CourseId = a.CourseId,
                        CourseName = a.Course != null ? a.Course.Id.ToString() : null,
                        Priority = a.Priority.ToString(),
                        PublishedAt = a.PublishedAt,
                        IsActive = a.IsActive
                    })
                    .ToListAsync();

                return dbAnnouncements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting announcements");
                throw;
            }
        }
        public async Task<List<AnnouncementModel>> GetLatestAnnouncementsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                // Only get active announcements from the database
                var dbAnnouncements = await context.Announcements
                    .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow))
                    .OrderByDescending(a => a.Id)
                    .Select(a => new AnnouncementModel
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Content = a.Content,
                        AuthorId = a.AuthorId,
                        AuthorName = a.Author.FullName,
                        CourseId = a.CourseId,
                        CourseName = a.Course != null ? a.Course.Id.ToString() : null,
                        Priority = a.Priority.ToString(),
                        PublishedAt = a.PublishedAt,
                        IsActive = a.IsActive
                    })
                    .Take(5)
                    .ToListAsync();

                return dbAnnouncements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest announcements");
                throw;
            }
        }

        public async Task<List<AnnouncementModel>> GetAnnouncementsByCourseAsync(int courseId)
        {
            try
            {
                var allAnnouncements = await GetAnnouncementsAsync();
                return allAnnouncements.Where(a => a.CourseId == courseId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting announcements by course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<AnnouncementModel?> GetAnnouncementByIdAsync(int id)
        {
            try
            {
                var allAnnouncements = await GetAnnouncementsAsync();
                return allAnnouncements.FirstOrDefault(a => a.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting announcement by id: {AnnouncementId}", id);
                throw;
            }
        }

        public async Task<AnnouncementModel> CreateAnnouncementAsync(CreateAnnouncementRequest request)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var announcement = new Announcement
                {
                    Title = request.Title,
                    Content = request.Content,
                    Priority = request.Priority,
                    PublishedAt = DateTime.UtcNow,
                    IsActive = true,
                    SendEmail = request.SendEmail,
                    SendSms = request.SendSms,
                    CourseId = request.CourseId
                };
                context.Announcements.Add(announcement);
                await context.SaveChangesAsync();
                return new AnnouncementModel
                {
                    Id = announcement.Id,
                    Title = announcement.Title,
                    Content = announcement.Content,
                    Priority = announcement.Priority.ToString(),
                    PublishedAt = announcement.PublishedAt,
                    IsActive = announcement.IsActive,
                    SendEmail = announcement.SendEmail,
                    SendSms = announcement.SendSms,
                    CourseId = announcement.CourseId,
                    AuthorName = announcement.AuthorId // Replace with actual user name if needed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating announcement");
                throw;
            }
        }

        public async Task<AnnouncementModel> UpdateAnnouncementAsync(int id, CreateAnnouncementRequest request)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var announcement = await context.Announcements.FindAsync(id);
                if (announcement == null)
                    throw new ArgumentException("Announcement not found");
                announcement.Title = request.Title;
                announcement.Content = request.Content;
                announcement.Priority = request.Priority;
                await context.SaveChangesAsync();
                return new AnnouncementModel
                {
                    Id = announcement.Id,
                    Title = announcement.Title,
                    Content = announcement.Content,
                    Priority = announcement.Priority.ToString(),
                    PublishedAt = announcement.PublishedAt,
                    IsActive = announcement.IsActive,
                    SendEmail = announcement.SendEmail,
                    SendSms = announcement.SendSms,
                    CourseId = announcement.CourseId,
                    AuthorName = announcement.AuthorId // Replace with actual user name if needed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating announcement: {AnnouncementId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAnnouncementAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var announcement = await context.Announcements.FindAsync(id);
                if (announcement == null)
                    return false;
                context.Announcements.Remove(announcement);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting announcement: {AnnouncementId}", id);
                throw;
            }
        }

        public async Task<List<AnnouncementModel>> GetFilteredAnnouncementsAsync(string? searchTerm, string? priority, string? sortBy)
        {
            try
            {
                var all = await GetAnnouncementsAsync();
                IEnumerable<AnnouncementModel> filtered = all;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    filtered = filtered.Where(a =>
                        (a.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.Content?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.AuthorName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                if (!string.IsNullOrEmpty(priority))
                {
                    filtered = filtered.Where(a => a.Priority == priority);
                }

                filtered = sortBy switch
                {
                    "oldest" => filtered.OrderBy(a => a.PublishedAt),
                    "priority" => filtered.OrderByDescending(a => GetPriorityWeight(a.Priority))
                                        .ThenByDescending(a => a.Priority == "Critical")
                                        .ThenByDescending(a => a.PublishedAt),
                    _ => filtered.OrderByDescending(a => a.Priority == "High")
                                .ThenByDescending(a => a.Priority == "Critical")
                                .ThenByDescending(a => a.PublishedAt)
                };

                return filtered.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered announcements");
                throw;
            }
        }

        public async Task<List<AnnouncementModel>> GetAllAnnouncementsAsync()
        {
            // For compatibility, just call GetAnnouncementsAsync
            return await GetAnnouncementsAsync();
        }

        private int GetPriorityWeight(string priority) => priority switch
        {
            "Critical" => 4,
            "High" => 3,
            "Medium" => 2,
            "Low" => 1,
            _ => 0
        };

        private string GetPriorityString(int priority) => priority switch
        {
            1 => "Low",
            2 => "Medium",
            3 => "High",
            4 => "Critical",
            _ => "Medium"
        };
    }
}