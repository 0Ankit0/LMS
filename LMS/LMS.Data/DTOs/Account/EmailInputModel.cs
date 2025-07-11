using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class EmailInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
