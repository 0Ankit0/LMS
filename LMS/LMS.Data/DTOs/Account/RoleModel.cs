using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class RoleModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
