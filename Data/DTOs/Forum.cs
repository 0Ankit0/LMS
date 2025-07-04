using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data
{
    public class Forum
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int? CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        public bool IsGeneral { get; set; } = false; // General forums not tied to courses

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<ForumTopic> Topics { get; set; } = new List<ForumTopic>();
    }

    public class ForumTopic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public int ForumId { get; set; }

        [ForeignKey("ForumId")]
        public virtual Forum Forum { get; set; } = null!;

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; } = null!;

        public bool IsPinned { get; set; } = false;

        public bool IsLocked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastPostAt { get; set; }

        // Navigation Properties
        public virtual ICollection<ForumPost> Posts { get; set; } = new List<ForumPost>();
    }

    public class ForumPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public int TopicId { get; set; }

        [ForeignKey("TopicId")]
        public virtual ForumTopic Topic { get; set; } = null!;

        [Required]
        public string AuthorId { get; set; } = string.Empty;

        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; } = null!;

        public int? ParentPostId { get; set; }

        [ForeignKey("ParentPostId")]
        public virtual ForumPost? ParentPost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<ForumPost> Replies { get; set; } = new List<ForumPost>();
        public virtual ICollection<ForumAttachment> Attachments { get; set; } = new List<ForumAttachment>();
    }

    public class ForumAttachment
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual ForumPost Post { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Caption { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string UploadedByUserId { get; set; } = string.Empty;

        [ForeignKey("UploadedByUserId")]
        public virtual User UploadedBy { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;
    }
}
