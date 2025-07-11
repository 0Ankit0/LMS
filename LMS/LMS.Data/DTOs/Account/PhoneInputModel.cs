using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class PhoneInputModel
    {
        [Required]
        [Phone]
        public string NewPhone { get; set; } = string.Empty;
    }
}
