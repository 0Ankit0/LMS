using LMS.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public string Color { get; set; } = "#007bff";
        public bool IsActive { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public List<CategoryModel> SubCategories { get; set; } = new();
        public int CourseCount { get; set; }
    }

    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public string Color { get; set; } = "#007bff";
        public int? ParentCategoryId { get; set; }
    }

    public interface ICategoryService
    {
        Task<List<CategoryModel>> GetCategoriesAsync();
        Task<List<CategoryModel>> GetRootCategoriesAsync();
        Task<CategoryModel?> GetCategoryByIdAsync(int id);
        Task<List<CategoryModel>> GetSubCategoriesAsync(int parentCategoryId);
        Task<CategoryModel> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryModel> UpdateCategoryAsync(int id, CreateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
        Task<List<CategoryModel>> GetCategoriesByCourseIdAsync(int courseId);
    }

    public class CategoryService : ICategoryService
    {
        private readonly AuthDbContext _context;

        public CategoryService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryModel>> GetCategoriesAsync()
        {
            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.CourseCategories)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(MapToCategoryModel).ToList();
        }

        public async Task<List<CategoryModel>> GetRootCategoriesAsync()
        {
            var categories = await _context.Categories
                .Include(c => c.SubCategories)
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
                .Include(c => c.SubCategories)
                .Include(c => c.CourseCategories)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            return category != null ? MapToCategoryModel(category) : null;
        }

        public async Task<List<CategoryModel>> GetSubCategoriesAsync(int parentCategoryId)
        {
            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
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
                IconUrl = request.IconUrl,
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
            category.IconUrl = request.IconUrl;
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
                IconUrl = category.IconUrl,
                Color = category.Color,
                IsActive = category.IsActive,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                SubCategories = category.SubCategories?.Where(sc => sc.IsActive).Select(sc => new CategoryModel
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    IconUrl = sc.IconUrl,
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
