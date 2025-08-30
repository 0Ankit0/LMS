using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using LMS.Data.DTOs;
using LMS.Web.Data;
using Microsoft.Extensions.Logging;

namespace LMS.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<CategoryModel>> GetCategoriesAsync();
        Task<PaginatedResult<CategoryModel>> GetCategoriesPaginatedAsync(PaginationRequest request);
        Task<List<CategoryModel>> GetRootCategoriesAsync();
        Task<CategoryModel?> GetCategoryByIdAsync(int id);
        Task<List<CategoryModel>> GetSubCategoriesAsync(int parentCategoryId);
        Task<CategoryModel> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryModel> UpdateCategoryAsync(int id, CreateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
        Task<List<CategoryModel>> GetCategoriesByCourseIdAsync(int courseId);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(ApplicationDbContext context, ILogger<CategoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<CategoryModel>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.ParentCategory)
                    .Include(c => c.IconFile)
                    .Include(c => c.SubCategories)
                        .ThenInclude(sc => sc.IconFile)
                    .Include(c => c.CourseCategories)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return categories.Select(MapToCategoryModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                throw;
            }
        }

        public async Task<PaginatedResult<CategoryModel>> GetCategoriesPaginatedAsync(PaginationRequest request)
        {
            request.Validate();
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.IconFile)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.IconFile)
                .Include(c => c.CourseCategories)
                .OrderBy(c => c.Name);

            var totalCount = await query.CountAsync();
            var skip = (request.PageNumber - 1) * request.PageSize;
            var categories = await query.Skip(skip).Take(request.PageSize).ToListAsync();

            return new PaginatedResult<CategoryModel>
            {
                Items = categories.Select(MapToCategoryModel).ToList(),
                TotalCount = totalCount,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };
        }

        public async Task<List<CategoryModel>> GetRootCategoriesAsync()
        {
            var categories = await _context.Categories
                .Include(c => c.IconFile)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.IconFile)
                .Include(c => c.CourseCategories)
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(MapToCategoryModel).ToList();
        }

        public async Task<CategoryModel?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.IconFile)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.IconFile)
                .Include(c => c.CourseCategories)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            return category != null ? MapToCategoryModel(category) : null;
        }

        public async Task<List<CategoryModel>> GetSubCategoriesAsync(int parentCategoryId)
        {
            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.IconFile)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.IconFile)
                .Include(c => c.CourseCategories)
                .Where(c => c.ParentCategoryId == parentCategoryId && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(MapToCategoryModel).ToList();
        }

        public async Task<CategoryModel> CreateCategoryAsync(CreateCategoryRequest request)
        {
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                
                Color = request.Color,
                ParentCategoryId = request.ParentCategoryId,
                IsActive = true
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return await GetCategoryByIdAsync(category.Id) ?? throw new InvalidOperationException("Failed to retrieve created category");
        }

        public async Task<CategoryModel> UpdateCategoryAsync(int id, CreateCategoryRequest request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                throw new ArgumentException("Category not found", nameof(id));
            category.Name = request.Name;
            category.Description = request.Description;
            
            category.Color = request.Color;
            category.ParentCategoryId = request.ParentCategoryId;
            await _context.SaveChangesAsync();
            return await GetCategoryByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated category");
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.CourseCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return false;

            // Check if category has courses or subcategories
            if (category.SubCategories.Any() || category.CourseCategories.Any())
            {
                category.IsActive = false; // Soft delete
            }
            else
            {
                _context.Categories.Remove(category); // Hard delete
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CategoryModel>> GetCategoriesByCourseIdAsync(int courseId)
        {
            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.IconFile)
                .Include(c => c.CourseCategories)
                .Where(c => c.CourseCategories.Any(cc => cc.CourseId == courseId) && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(MapToCategoryModel).ToList();
        }

        private static CategoryModel MapToCategoryModel(Category category)
        {
            return new CategoryModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IconUrl = category.IconFile?.FilePath ?? string.Empty,
                Color = category.Color,
                IsActive = category.IsActive,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                SubCategories = category.SubCategories?.Where(sc => sc.IsActive).Select(sc => new CategoryModel
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    IconUrl = sc.IconFile?.FilePath ?? string.Empty,
                    Color = sc.Color,
                    IsActive = sc.IsActive,
                    ParentCategoryId = sc.ParentCategoryId,
                    CourseCount = sc.CourseCategories?.Count ?? 0
                }).ToList() ?? new List<CategoryModel>(),
                CourseCount = category.CourseCategories?.Count ?? 0
            };
        }
    }
}
