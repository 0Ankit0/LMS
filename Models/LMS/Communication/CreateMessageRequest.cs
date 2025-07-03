using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Communication
{
    public class CreateMessageRequest
    {
        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string ToUserId { get; set; } = string.Empty;

        public int? ParentMessageId { get; set; }

        public int Priority { get; set; } = 2; // Normal priority
    }
}
