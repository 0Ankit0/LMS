using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services
{
    public interface IAnnouncementService
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
    }

    public class AnnouncementService : IAnnouncementService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public AnnouncementService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<AnnouncementModel>> GetAnnouncementsAsync()
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

            // Add dummy data
            dbAnnouncements.AddRange(new List<AnnouncementModel>
            {
                new()
                {
                    Id = -1,
                    Title = "Welcome to the LMS Platform",
                    Content = "We're excited to have you join our learning management system. Explore the courses, complete assignments, and track your progress.",
                    AuthorId = "admin",
                    AuthorName = "System Administrator",
                    Priority = "Critical",
                    PublishedAt = DateTime.Now.AddDays(-1),
                    IsActive = true
                },
                new()
                {
                    Id = -2,
                    Title = "New Course Available: Machine Learning Basics",
                    Content = "A new course on Machine Learning fundamentals is now available. Enroll today to get started with AI and data science.",
                    AuthorId = "instructor1",
                    AuthorName = "Dr. Sarah Johnson",
                    CourseId = 1,
                    CourseName = "Machine Learning Basics",
                    Priority = "High",
                    PublishedAt = DateTime.Now.AddDays(-3),
                    IsActive = true
                },
                new()
                {
                    Id = -3,
                    Title = "System Maintenance Scheduled",
                    Content = "The system will undergo scheduled maintenance this weekend from 2 AM to 6 AM EST. Some features may be temporarily unavailable.",
                    AuthorId = "admin",
                    AuthorName = "System Administrator",
                    Priority = "Medium",
                    PublishedAt = DateTime.Now.AddDays(-5),
                    IsActive = true
                },
                new()
                {
                    Id = -4,
                    Title = "Tips for Effective Online Learning",
                    Content = "Here are some proven strategies to maximize your learning experience: 1) Set a regular study schedule, 2) Take notes actively, 3) Participate in discussions, 4) Seek help when needed.",
                    AuthorId = "instructor2",
                    AuthorName = "Prof. Michael Chen",
                    Priority = "Low",
                    PublishedAt = DateTime.Now.AddDays(-7),
                    IsActive = true
                }
            });

            return dbAnnouncements;
        }
        public async Task<List<AnnouncementModel>> GetLatestAnnouncementsAsync()
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

            // Add dummy data
            dbAnnouncements.AddRange(new List<AnnouncementModel>
            {
                new()
                {
                    Id = -1,
                    Title = "Welcome to the LMS Platform",
                    Content = "We're excited to have you join our learning management system. Explore the courses, complete assignments, and track your progress.",
                    AuthorId = "admin",
                    AuthorName = "System Administrator",
                    Priority = "Critical",
                    PublishedAt = DateTime.Now.AddDays(-1),
                    IsActive = true
                },
                new()
                {
                    Id = -2,
                    Title = "New Course Available: Machine Learning Basics",
                    Content = "A new course on Machine Learning fundamentals is now available. Enroll today to get started with AI and data science.",
                    AuthorId = "instructor1",
                    AuthorName = "Dr. Sarah Johnson",
                    CourseId = 1,
                    CourseName = "Machine Learning Basics",
                    Priority = "High",
                    PublishedAt = DateTime.Now.AddDays(-3),
                    IsActive = true
                },
                new()
                {
                    Id = -3,
                    Title = "System Maintenance Scheduled",
                    Content = "The system will undergo scheduled maintenance this weekend from 2 AM to 6 AM EST. Some features may be temporarily unavailable.",
                    AuthorId = "admin",
                    AuthorName = "System Administrator",
                    Priority = "Medium",
                    PublishedAt = DateTime.Now.AddDays(-5),
                    IsActive = true
                },
                new()
                {
                    Id = -4,
                    Title = "Tips for Effective Online Learning",
                    Content = "Here are some proven strategies to maximize your learning experience: 1) Set a regular study schedule, 2) Take notes actively, 3) Participate in discussions, 4) Seek help when needed.",
                    AuthorId = "instructor2",
                    AuthorName = "Prof. Michael Chen",
                    Priority = "Low",
                    PublishedAt = DateTime.Now.AddDays(-7),
                    IsActive = true
                }
            });

            return dbAnnouncements;
        }

        public async Task<List<AnnouncementModel>> GetAnnouncementsByCourseAsync(int courseId)
        {
            var allAnnouncements = await GetAnnouncementsAsync();
            return allAnnouncements.Where(a => a.CourseId == courseId).ToList();
        }

        public async Task<AnnouncementModel?> GetAnnouncementByIdAsync(int id)
        {
            var allAnnouncements = await GetAnnouncementsAsync();
            return allAnnouncements.FirstOrDefault(a => a.Id == id);
        }

        public async Task<AnnouncementModel> CreateAnnouncementAsync(CreateAnnouncementRequest request)
        {
            await Task.Delay(100); // Simulate async operation

            return new AnnouncementModel
            {
                Id = new Random().Next(1000, 9999),
                Title = request.Title,
                Content = request.Content,
                Priority = request.Priority.ToString(),
                PublishedAt = DateTime.Now,
                IsActive = true,
                SendEmail = request.SendEmail,
                SendSms = request.SendSms,
                CourseId = request.CourseId,
                AuthorName = "Current User" // Replace with actual user
            };
        }

        public async Task<AnnouncementModel> UpdateAnnouncementAsync(int id, CreateAnnouncementRequest request)
        {
            await Task.Delay(100); // Simulate async operation

            var announcement = await GetAnnouncementByIdAsync(id);
            if (announcement == null)
                throw new ArgumentException("Announcement not found");

            announcement.Title = request.Title;
            announcement.Content = request.Content;
            announcement.Priority = request.Priority.ToString();

            return announcement;
        }

        public async Task<bool> DeleteAnnouncementAsync(int id)
        {
            await Task.Delay(100); // Simulate async operation
            return true; // Always success for mock
        }

        public async Task<List<AnnouncementModel>> GetFilteredAnnouncementsAsync(string? searchTerm, string? priority, string? sortBy)
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