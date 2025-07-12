using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class EnableAuthenticatorInputModel
    {
        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
