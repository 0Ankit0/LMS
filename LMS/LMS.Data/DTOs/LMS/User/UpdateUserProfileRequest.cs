using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class UpdateUserProfileRequest
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Bio { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? ProfilePictureUrl { get; set; }

        [Required]
        public string Role { get; set; } = "Student";
    }
}