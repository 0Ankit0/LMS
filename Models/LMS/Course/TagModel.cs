using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Course
{
    public class TagModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#6c757d";
        public bool IsActive { get; set; }
        public int CourseCount { get; set; }
    }
}
