using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class ConversationModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsGroupConversation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessage { get; set; }
        public List<ConversationParticipantModel> Participants { get; set; } = new();
        public int UnreadCount { get; set; }
    }

    public class ConversationParticipantModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public record CreateConversationRequest(
        [Required] List<string> ParticipantIds,
        string? Title = null,
        string? Description = null,
        bool IsGroupConversation = false
    );

    public class MessageModel
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? EncryptedContent { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public List<MessageAttachmentModel> Attachments { get; set; } = new();

        // Additional properties used in components
        public string? FromUserName { get; set; }
        public string? ToUserName { get; set; }
        public string? Subject { get; set; }
        public string? Priority { get; set; }
        public string? FromUserId { get; set; }
        public List<MessageModel>? Replies { get; set; }
    }

    public class MessageAttachmentModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }

    public record SendMessageRequest(
        [Required] int ConversationId,
        [Required] string Content,
        string? EncryptedContent = null,
        List<int>? AttachmentIds = null
    );

    public class ForumTopicModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public int PostCount { get; set; }
        public DateTime? LastPostAt { get; set; }
        public string? LastPostBy { get; set; }

        // Additional properties used in components
        public int? ForumId { get; set; }
        public string? ForumTitle { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public List<ForumPostModel>? Posts { get; set; }
    }

    public class ForumPostModel
    {
        public int Id { get; set; }
        public int TopicId { get; set; }
        public string TopicTitle { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public int? ParentPostId { get; set; }
        public List<ForumPostModel> Replies { get; set; } = new();
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        // Additional properties used in components
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateForumPostRequest
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        public int? ParentPostId { get; set; }
        public int? TopicId { get; set; }
    }
}
