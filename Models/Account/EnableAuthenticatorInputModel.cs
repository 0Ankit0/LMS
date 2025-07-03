using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Account
{
    public class EnableAuthenticatorInputModel
    {
        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
