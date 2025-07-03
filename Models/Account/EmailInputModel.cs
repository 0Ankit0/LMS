using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Account
{
    public class EmailInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
