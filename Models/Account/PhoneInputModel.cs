using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Account
{
    public class PhoneInputModel
    {
        [Required]
        [Phone]
        public string NewPhone { get; set; } = string.Empty;
    }
}
