using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? IconUrl { get; set; }

        public string Color { get; set; } = "#007bff";

        public bool IsActive { get; set; } = true;

        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public virtual Category? ParentCategory { get; set; }

        // Navigation Properties
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
    }

    public class CourseCategory
    {
        public int CourseId { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;
    }

    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public string Color { get; set; } = "#6c757d";

        // Navigation Properties
        public virtual ICollection<CourseTags> CourseTags { get; set; } = new List<CourseTags>();
    }

    public class CourseTags
    {
        public int CourseId { get; set; }
        public int TagId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; } = null!;
    }
}
