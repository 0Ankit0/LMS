using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class PasswordInputModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
