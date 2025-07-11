using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class CreateForumRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int? CourseId { get; set; }

        public bool IsGeneral { get; set; } = false;
    }
}
