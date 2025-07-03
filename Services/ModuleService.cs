using LMS.Data;
using LMS.Models.Course;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LMS.Services
{
    public interface IModuleService
    {
        Task<List<ModuleModel>> GetModulesAsync();
        Task<ModuleModel?> GetModuleByIdAsync(int id);
        Task<List<ModuleModel>> GetModulesByCourseIdAsync(int courseId);
        Task<ModuleModel> CreateModuleAsync(CreateModuleRequest request);
        Task<ModuleModel> UpdateModuleAsync(int id, CreateModuleRequest request);
        Task<bool> DeleteModuleAsync(int id);
        Task<bool> UpdateModuleOrderAsync(int moduleId, int newOrder);
    }

    public class ModuleService : IModuleService
    {
        private readonly IDbContextFactory<AuthDbContext> _contextFactory;

        public ModuleService(IDbContextFactory<AuthDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ModuleModel>> GetModulesAsync()
        {
            await using var _context = _contextFactory.CreateDbContext();
            var modules = await _context.Modules
                .Include(m => m.Course)
                .Include(m => m.Lessons)
                .OrderBy(m => m.CourseId)
                .ThenBy(m => m.OrderIndex)
                .ToListAsync();

            return modules.Select(MapToModuleModel).ToList();
        }

        public async Task<ModuleModel?> GetModuleByIdAsync(int id)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var module = await _context.Modules
                .Include(m => m.Course)
                .Include(m => m.Lessons.OrderBy(l => l.OrderIndex))
                .FirstOrDefaultAsync(m => m.Id == id);

            return module != null ? MapToModuleModel(module) : null;
        }

        public async Task<List<ModuleModel>> GetModulesByCourseIdAsync(int courseId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var modules = await _context.Modules
                .Include(m => m.Course)
                .Include(m => m.Lessons)
                .Where(m => m.CourseId == courseId)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();

            return modules.Select(MapToModuleModel).ToList();
        }

        public async Task<ModuleModel> CreateModuleAsync(CreateModuleRequest request)
        {
            await using var _context = _contextFactory.CreateDbContext();
            // Get the next order index for this course if not specified
            int orderIndex = request.OrderIndex;
            if (orderIndex <= 0)
            {
                var maxOrder = await _context.Modules
                    .Where(m => m.CourseId == request.CourseId)
                    .MaxAsync(m => (int?)m.OrderIndex) ?? 0;
                orderIndex = maxOrder + 1;
            }

            var module = new Module
            {
                Title = request.Title,
                Description = request.Description,
                CourseId = request.CourseId,
                OrderIndex = orderIndex,
                IsRequired = request.IsRequired,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Modules.Add(module);
            await _context.SaveChangesAsync();

            return await GetModuleByIdAsync(module.Id) ?? throw new InvalidOperationException("Failed to retrieve created module");
        }

        public async Task<ModuleModel> UpdateModuleAsync(int id, CreateModuleRequest request)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var module = await _context.Modules.FindAsync(id);
            if (module == null)
                throw new ArgumentException("Module not found", nameof(id));

            module.Title = request.Title;
            module.Description = request.Description;
            module.OrderIndex = request.OrderIndex;
            module.IsRequired = request.IsRequired;
            module.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetModuleByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated module");
        }

        public async Task<bool> DeleteModuleAsync(int id)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var module = await _context.Modules.FindAsync(id);
            if (module == null)
                return false;

            _context.Modules.Remove(module);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateModuleOrderAsync(int moduleId, int newOrder)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var module = await _context.Modules.FindAsync(moduleId);
            if (module == null)
                return false;

            var oldOrder = module.OrderIndex;
            module.OrderIndex = newOrder;
            module.UpdatedAt = DateTime.UtcNow;

            // Update other modules in the same course
            var otherModules = await _context.Modules
                .Where(m => m.CourseId == module.CourseId && m.Id != moduleId)
                .ToListAsync();

            if (newOrder > oldOrder)
            {
                // Moving down - shift modules up
                foreach (var otherModule in otherModules.Where(m => m.OrderIndex > oldOrder && m.OrderIndex <= newOrder))
                {
                    otherModule.OrderIndex--;
                    otherModule.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (newOrder < oldOrder)
            {
                // Moving up - shift modules down
                foreach (var otherModule in otherModules.Where(m => m.OrderIndex >= newOrder && m.OrderIndex < oldOrder))
                {
                    otherModule.OrderIndex++;
                    otherModule.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private static ModuleModel MapToModuleModel(Module module)
        {
            return new ModuleModel
            {
                Id = module.Id,
                Title = module.Title,
                Description = module.Description,
                CourseId = module.CourseId,
                OrderIndex = module.OrderIndex,
                IsRequired = module.IsRequired,
                IsActive = module.IsActive,
                Lessons = module.Lessons?.OrderBy(l => l.OrderIndex).Select(l => new LessonModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    ModuleId = l.ModuleId,
                    OrderIndex = l.OrderIndex,
                    Content = l.Content,
                    VideoUrl = l.VideoUrl,
                    IsActive = l.IsActive
                }).ToList() ?? new List<LessonModel>()
            };
        }
    }
}
