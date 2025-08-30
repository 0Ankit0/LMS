using LMS.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs.LMS.Note
{
    public class NoteDTO
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public int? CourseId { get; set; }
        public string? CourseTitle { get; set; }

        public int? LessonId { get; set; }
        public string? LessonTitle { get; set; }

        public bool IsPrivate { get; set; } = true;
        public bool IsPinned { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string? Tags { get; set; }
        public NoteType Type { get; set; } = NoteType.General;
        public NotePriority Priority { get; set; } = NotePriority.Normal;

        // Helper properties
        public string TypeDisplay => Type.ToString();
        public string PriorityDisplay => Priority.ToString();
        public string CreatedAtDisplay => CreatedAt.ToString("MMM dd, yyyy HH:mm");
        public string UpdatedAtDisplay => UpdatedAt.ToString("MMM dd, yyyy HH:mm");
        public bool IsRecent => CreatedAt >= DateTime.UtcNow.AddDays(-7);
        public List<string> TagList => string.IsNullOrEmpty(Tags) ? new List<string>() : Tags.Split(',').Select(t => t.Trim()).ToList();
    }

    public class CreateNoteDTO
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? CourseId { get; set; }
        public int? LessonId { get; set; }
        public bool IsPrivate { get; set; } = true;
        public bool IsPinned { get; set; } = false;
        public string? Tags { get; set; }
        public NoteType Type { get; set; } = NoteType.General;
        public NotePriority Priority { get; set; } = NotePriority.Normal;
    }

    public class UpdateNoteDTO
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? CourseId { get; set; }
        public int? LessonId { get; set; }
        public bool IsPrivate { get; set; } = true;
        public bool IsPinned { get; set; } = false;
        public string? Tags { get; set; }
        public NoteType Type { get; set; } = NoteType.General;
        public NotePriority Priority { get; set; } = NotePriority.Normal;
    }

    public class NoteFilterDTO
    {
        public string? SearchTerm { get; set; }
        public int? CourseId { get; set; }
        public int? LessonId { get; set; }
        public NoteType? Type { get; set; }
        public NotePriority? Priority { get; set; }
        public string? Tags { get; set; }
        public bool? IsPrivate { get; set; }
        public bool? IsPinned { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class NotesSummaryDTO
    {
        public int TotalNotes { get; set; }
        public int PinnedNotes { get; set; }
        public int PrivateNotes { get; set; }
        public int PublicNotes { get; set; }
        public int RecentNotes { get; set; }
        public Dictionary<NoteType, int> NotesByType { get; set; } = new();
        public Dictionary<NotePriority, int> NotesByPriority { get; set; } = new();
        public List<string> MostUsedTags { get; set; } = new();
    }

    public class NoteQuickAccessDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CourseTitle { get; set; }
        public NoteType Type { get; set; }
        public NotePriority Priority { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Aliases for endpoint compatibility
    public class CreateNoteRequest : CreateNoteDTO { }
    public class UpdateNoteRequest : UpdateNoteDTO { }
    public class GetNotesRequest : NoteFilterDTO
    {
        public string? UserId { get; set; }
    }
}
