using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Account
{
    public class RoleModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
