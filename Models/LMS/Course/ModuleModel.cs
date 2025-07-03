using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Course
{
    public class ModuleModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int CourseId { get; set; }

        public int OrderIndex { get; set; }

        public bool IsRequired { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public List<LessonModel> Lessons { get; set; } = new();

        public double ProgressPercentage { get; set; }
    }
}
