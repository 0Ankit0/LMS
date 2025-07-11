using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class AssignUserModel
    {
        [Required(ErrorMessage = "User email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string UserEmail { get; set; } = string.Empty;
    }
}
