using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Account
{
    public class PasswordInputModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
