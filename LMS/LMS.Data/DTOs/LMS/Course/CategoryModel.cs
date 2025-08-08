using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
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
        public string? IconName { get; set; } // Add for icon support
    }

    public class CreateCategoryRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public string Color { get; set; } = "#007bff";
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? IconName { get; set; } // Add for icon support
    }



    public class CreateTagRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
        public string Color { get; set; } = "#6c757d";
        public bool IsActive { get; set; } = true;
    }
}
