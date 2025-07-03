namespace LMS.Models.Course
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

    public class TagModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#6c757d";
        public bool IsActive { get; set; }
        public int CourseCount { get; set; }
    }

    public class CreateTagRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#6c757d";
    }
}
