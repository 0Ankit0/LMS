using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data
{
    // Communication Models
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string FromUserId { get; set; } = string.Empty;

        [ForeignKey("FromUserId")]
        public virtual User FromUser { get; set; } = null!;

        [Required]
        public string ToUserId { get; set; } = string.Empty;

        [ForeignKey("ToUserId")]
        public virtual User ToUser { get; set; } = null!;

        public int? ParentMessageId { get; set; }

        [ForeignKey("ParentMessageId")]
        public virtual Message? ParentMessage { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public MessagePriority Priority { get; set; } = MessagePriority.Normal;

        // Navigation Properties
        public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
        public virtual ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
    }

    public class MessageAttachment
    {
        [Key]
        public int Id { get; set; }

        public int MessageId { get; set; }

        [ForeignKey("MessageId")]
        public virtual Message Message { get; set; } = null!;

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string ContentType { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string AuthorId { get; set; } = string.Empty;

        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; } = null!;

        public int? CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        // Add Type property
        [Required]
        public AnnouncementType Type { get; set; } = AnnouncementType.General;

        public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public bool SendEmail { get; set; } = false;

        public bool SendSms { get; set; } = false;
    }

    public enum MessagePriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }

    public enum AnnouncementPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    // Add AnnouncementType enum
    public enum AnnouncementType
    {
        General = 1,
        Event = 2,
        Deadline = 3,
        Alert = 4
    }
}
