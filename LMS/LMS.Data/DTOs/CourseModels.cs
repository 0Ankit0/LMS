using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class CourseModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string Level { get; set; } = string.Empty; // Beginner, Intermediate, Advanced, Expert
        public decimal? Price { get; set; }
        public bool IsFree { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string InstructorId { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int Duration { get; set; } // in minutes
        public int ModuleCount { get; set; }
        public int LessonCount { get; set; }
        public int EnrollmentCount { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<string> Prerequisites { get; set; } = new();
        public List<string> LearningObjectives { get; set; } = new();
        public bool IsEnrolled { get; set; }
        public decimal? UserProgress { get; set; }
        public bool IsCompleted { get; set; }
        public int ProgressPercentage { get; set; }

        // Additional properties for dashboard and admin views
        public string Status { get; set; } = string.Empty;
        public int? MaxEnrollments { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? EstimatedDuration { get; set; }
        public List<string> Categories { get; set; } = new();
        public List<AssessmentModel> Assessments { get; set; } = new();
        public List<ModuleModel> Modules { get; set; } = new();
        public string? IconName { get; set; }
    }

    public class CreateCourseRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ShortDescription { get; set; }

        public string? ThumbnailUrl { get; set; }

        [Required]
        public string Level { get; set; } = string.Empty;

        public decimal? Price { get; set; }
        public bool IsFree { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? InstructorId { get; set; }
        public List<string>? Tags { get; set; }
        public List<string>? Prerequisites { get; set; }
        public List<string>? LearningObjectives { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        // Additional properties used in components
        public string? Status { get; set; }
        public int? MaxEnrollments { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? EstimatedDuration { get; set; }
        public List<string>? CategoryIds { get; set; }
        public List<string>? TagIds { get; set; }
        public string? IconName { get; set; }
    }

    public record UpdateCourseRequest(
        [StringLength(200)] string? Title,
        [StringLength(2000)] string? Description,
        [StringLength(500)] string? ShortDescription,
        string? ThumbnailUrl,
        string? Level,
        decimal? Price,
        bool? IsFree,
        int? CategoryId,
        string? InstructorId,
        List<string>? Tags,
        List<string>? Prerequisites,
        List<string>? LearningObjectives,
        bool? IsActive,
        bool? IsFeatured
    );

    public record GetCoursesRequest(
        string? Search = null,
        int? CategoryId = null,
        string? Level = null,
        bool? IsActive = null,
        bool? IsFree = null,
        bool? IsFeatured = null,
        string? InstructorId = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        string? SortBy = null, // title, created, price, rating, popularity
        string? SortDirection = "asc", // asc, desc
        int Page = 1,
        int PageSize = 20
    );

    public class CourseDetailModel : CourseModel
    {
        public new List<ModuleModel> Modules { get; set; } = new();
        public List<CourseReviewModel> Reviews { get; set; } = new();
        public CourseStatsModel Stats { get; set; } = new();
    }

    public class CourseStatsModel
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int CompletedStudents { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageProgress { get; set; }
        public TimeSpan AverageCompletionTime { get; set; }
    }

    public class CourseReviewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVerifiedPurchase { get; set; }
    }

    public class ModuleModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
        public int Duration { get; set; } // in minutes
        public int CourseId { get; set; }
        public bool IsRequired { get; set; } = true;
        public string? ImageUrl { get; set; }
        public string? IconName { get; set; }
        public List<LessonModel> Lessons { get; set; } = new();
        public List<AssessmentModel> Assessments { get; set; } = new();
    }

    public class LessonModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty; // Video, Text, Quiz, Assignment
        public string LessonType { get; set; } = string.Empty; // Alias for Type
        public string? ContentUrl { get; set; }
        public string? Content { get; set; }
        public int OrderIndex { get; set; }
        public int Duration { get; set; } // in minutes
        public bool IsActive { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Additional properties used in components
        public int? ModuleId { get; set; }
        public string? VideoUrl { get; set; }
        public string? DocumentUrl { get; set; }
        public string? ExternalUrl { get; set; }
        public int? EstimatedDuration { get; set; } // in minutes
        public bool IsRequired { get; set; }
        public List<string>? Resources { get; set; }
        public List<DocumentModel>? Documents { get; set; } = new();
        public string? ModuleName { get; set; }
    }

    public class DocumentModel
    {
        public int Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class NoteModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int? CourseId { get; set; }
        public int? LessonId { get; set; }
        public int? ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsPrivate { get; set; } = true;
        public List<string> Tags { get; set; } = new();
        public string? CourseName { get; set; }
        public string? LessonName { get; set; }
        public string? ModuleName { get; set; }
    }
}
