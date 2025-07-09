using System.ComponentModel.DataAnnotations;

namespace LMS.Models.User
{
    public class CreateEnrollmentRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }
    }
}
