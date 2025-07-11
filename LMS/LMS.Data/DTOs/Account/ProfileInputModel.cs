using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class ProfileInputModel
    {
        [Required]
        public string NewUsername { get; set; } = string.Empty;
    }
}
