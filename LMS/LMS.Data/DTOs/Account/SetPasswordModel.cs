using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class SetPasswordModel
    {
        [Required]
        [MinLength(6)]
        public string? NewPassword { get; set; }
        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
