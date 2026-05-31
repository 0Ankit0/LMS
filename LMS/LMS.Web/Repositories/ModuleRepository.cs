using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using LMS.Data.DTOs;
using LMS.Web.Data;

namespace LMS.Repositories
{
    public interface IModuleRepository
    {
        Task<List<ModuleModel>> GetModulesAsync();
        Task<PaginatedResult<ModuleModel>> GetModulesPaginatedAsync(PaginationRequest request);
        Task<ModuleModel?> GetModuleByIdAsync(int id);
        Task<List<ModuleModel>> GetModulesByCourseIdAsync(int courseId);
        Task<ModuleModel> CreateModuleAsync(CreateModuleRequest request);
        Task<ModuleModel> UpdateModuleAsync(int id, CreateModuleRequest request);
        Task<bool> DeleteModuleAsync(int id);
        Task<bool> UpdateModuleOrderAsync(int moduleId, int newOrder);
    }

    public class ModuleRepository : IModuleRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ModuleRepository> _logger;

        public ModuleRepository(ApplicationDbContext context, ILogger<ModuleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<List<ModuleModel>> GetModulesAsync()
        {
            try
            {
                var modules = await _context.Modules
                    .Include(m => m.Course)
                    .Include(m => m.Lessons)
                    .OrderBy(m => m.CourseId)
                    .ThenBy(m => m.OrderIndex)
                    .ToListAsync();
                return modules.Select(MapToModuleModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting modules");
                throw;
            }
        }





        public async Task<PaginatedResult<ModuleModel>> GetModulesPaginatedAsync(PaginationRequest request)
        {
            try
            {
                var query = _context.Modules
                    .Include(m => m.Course)
                    .Include(m => m.Lessons)
                    .OrderBy(m => m.CourseId)
                    .ThenBy(m => m.OrderIndex);

                var totalCount = await query.CountAsync();
                var modules = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return new PaginatedResult<ModuleModel>
                {
                    Items = modules.Select(MapToModuleModel).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated modules");
                throw;
            }
        }


        public async Task<ModuleModel?> GetModuleByIdAsync(int id)
        {
            try
            {
                var module = await _context.Modules
                    .Include(m => m.Course)
                    .Include(m => m.Lessons.OrderBy(l => l.OrderIndex))
                    .FirstOrDefaultAsync(m => m.Id == id);
                return module != null ? MapToModuleModel(module) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting module by id: {Id}", id);
                throw;
            }
        }


        public async Task<List<ModuleModel>> GetModulesByCourseIdAsync(int courseId)
        {
            try
            {
                var modules = await _context.Modules
                    .Include(m => m.Course)
                    .Include(m => m.Lessons)
                    .Where(m => m.CourseId == courseId)
                    .OrderBy(m => m.OrderIndex)
                    .ToListAsync();
                return modules.Select(MapToModuleModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting modules by course id: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<ModuleModel> CreateModuleAsync(CreateModuleRequest request)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating module");
                throw;
            }
        }

        public async Task<ModuleModel> UpdateModuleAsync(int id, CreateModuleRequest request)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating module: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteModuleAsync(int id)
        {
            try
            {
                var module = await _context.Modules.FindAsync(id);
                if (module == null)
                    return false;

                _context.Modules.Remove(module);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting module: {Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateModuleOrderAsync(int moduleId, int newOrder)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating module order: {ModuleId}", moduleId);
                throw;
            }
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
