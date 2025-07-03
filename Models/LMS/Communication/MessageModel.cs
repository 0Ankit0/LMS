using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Communication
{
    public class MessageModel
    {
        public int Id { get; set; }

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string FromUserId { get; set; } = string.Empty;

        public string FromUserName { get; set; } = string.Empty;

        public string ToUserId { get; set; } = string.Empty;

        public string ToUserName { get; set; } = string.Empty;

        public int? ParentMessageId { get; set; }

        public DateTime SentAt { get; set; }

        public DateTime? ReadAt { get; set; }

        public bool IsRead => ReadAt.HasValue;

        public string Priority { get; set; } = string.Empty;

        public List<MessageModel> Replies { get; set; } = new();

        public List<MessageAttachmentModel> Attachments { get; set; } = new();
    }
}
