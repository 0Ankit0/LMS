using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class CreateEnrollmentRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }
    }
}
