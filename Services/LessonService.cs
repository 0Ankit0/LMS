using LMS.Data;
using LMS.Models.Course;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services
{
    public interface ILessonService
    {
        Task<List<LessonModel>> GetLessonsAsync();
        Task<LessonModel?> GetLessonByIdAsync(int id);
        Task<List<LessonModel>> GetLessonsByModuleIdAsync(int moduleId);
        Task<LessonModel> CreateLessonAsync(CreateLessonRequest request);
        Task<LessonModel> UpdateLessonAsync(int id, CreateLessonRequest request);
        Task<bool> DeleteLessonAsync(int id);
        Task<bool> UpdateLessonOrderAsync(int lessonId, int newOrder);
    }

    public class LessonService : ILessonService
    {
        private readonly AuthDbContext _context;

        public LessonService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<List<LessonModel>> GetLessonsAsync()
        {
            var lessons = await _context.Lessons
                .Include(l => l.Module)
                .Include(l => l.Resources)
                .OrderBy(l => l.ModuleId)
                .ThenBy(l => l.OrderIndex)
                .ToListAsync();

            return lessons.Select(MapToLessonModel).ToList();
        }

        public async Task<LessonModel?> GetLessonByIdAsync(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Module)
                .ThenInclude(m => m.Course)
                .Include(l => l.Resources)
                .FirstOrDefaultAsync(l => l.Id == id);

            return lesson != null ? MapToLessonModel(lesson) : null;
        }

        public async Task<List<LessonModel>> GetLessonsByModuleIdAsync(int moduleId)
        {
            var lessons = await _context.Lessons
                .Include(l => l.Module)
                .Include(l => l.Resources)
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();

            return lessons.Select(MapToLessonModel).ToList();
        }

        public async Task<LessonModel> CreateLessonAsync(CreateLessonRequest request)
        {
            // Get the next order index for this module
            var maxOrder = await _context.Lessons
                .Where(l => l.ModuleId == request.ModuleId)
                .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

            var lesson = new Lesson
            {
                Title = request.Title,
                Description = request.Description,
                ModuleId = request.ModuleId,
                OrderIndex = maxOrder + 1,
                Type = (LessonType)request.Type,
                Content = request.Content,
                VideoUrl = request.VideoUrl,
                DocumentUrl = request.DocumentUrl,
                ExternalUrl = request.ExternalUrl,
                EstimatedDuration = request.EstimatedDuration,
                IsRequired = request.IsRequired,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return await GetLessonByIdAsync(lesson.Id) ?? throw new InvalidOperationException("Failed to retrieve created lesson");
        }

        public async Task<LessonModel> UpdateLessonAsync(int id, CreateLessonRequest request)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
                throw new ArgumentException("Lesson not found", nameof(id));

            lesson.Title = request.Title;
            lesson.Description = request.Description;
            lesson.Type = (LessonType)request.Type;
            lesson.Content = request.Content;
            lesson.VideoUrl = request.VideoUrl;
            lesson.DocumentUrl = request.DocumentUrl;
            lesson.ExternalUrl = request.ExternalUrl;
            lesson.EstimatedDuration = request.EstimatedDuration;
            lesson.IsRequired = request.IsRequired;
            lesson.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetLessonByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated lesson");
        }

        public async Task<bool> DeleteLessonAsync(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
                return false;

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLessonOrderAsync(int lessonId, int newOrder)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null)
                return false;

            var oldOrder = lesson.OrderIndex;
            lesson.OrderIndex = newOrder;
            lesson.UpdatedAt = DateTime.UtcNow;

            // Update other lessons in the same module
            var otherLessons = await _context.Lessons
                .Where(l => l.ModuleId == lesson.ModuleId && l.Id != lessonId)
                .ToListAsync();

            if (newOrder > oldOrder)
            {
                // Moving down - shift lessons up
                foreach (var otherLesson in otherLessons.Where(l => l.OrderIndex > oldOrder && l.OrderIndex <= newOrder))
                {
                    otherLesson.OrderIndex--;
                    otherLesson.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (newOrder < oldOrder)
            {
                // Moving up - shift lessons down
                foreach (var otherLesson in otherLessons.Where(l => l.OrderIndex >= newOrder && l.OrderIndex < oldOrder))
                {
                    otherLesson.OrderIndex++;
                    otherLesson.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private static LessonModel MapToLessonModel(Lesson lesson)
        {
            return new LessonModel
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                ModuleId = lesson.ModuleId,
                OrderIndex = lesson.OrderIndex,
                Type = lesson.Type.ToString(),
                Content = lesson.Content,
                VideoUrl = lesson.VideoUrl,
                DocumentUrl = lesson.DocumentUrl,
                ExternalUrl = lesson.ExternalUrl,
                EstimatedDuration = lesson.EstimatedDuration,
                IsRequired = lesson.IsRequired,
                IsActive = lesson.IsActive,
                Resources = lesson.Resources?.Select(r => new LessonResourceModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Type = r.Type.ToString(),
                    FilePath = r.FilePath,
                    ExternalUrl = r.ExternalUrl,
                    FileSize = r.FileSize,
                    ContentType = r.ContentType,
                    IsDownloadable = r.IsDownloadable
                }).ToList() ?? new List<LessonResourceModel>()
            };
        }
    }
}
