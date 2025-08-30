using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LMS.Web.DTOs.FileManagement
{
    public class UserFileModel
    {
        public int Id { get; set; }
        public string OwnerUserId { get; set; } = string.Empty;
        public string OwnerUserName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public bool IsActive { get; set; }
        public string FileSizeFormatted { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
    }

    public class UploadFileRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [StringLength(255)]
        public string? CustomFileName { get; set; }

        public bool IsPublic { get; set; } = false;
    }

    public class FileUploadResult
    {
        public bool Success { get; set; }
        public int? FileId { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileUrl { get; set; }
        public long FileSize { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class FileSearchRequest
    {
        public string? SearchTerm { get; set; }
        public string? ContentType { get; set; }
        public string? OwnerUserId { get; set; }
        public DateTime? UploadedAfter { get; set; }
        public DateTime? UploadedBefore { get; set; }
        public long? MinSize { get; set; }
        public long? MaxSize { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class BulkFileOperationRequest
    {
        public List<int> FileIds { get; set; } = new();
        public string Operation { get; set; } = string.Empty; // "delete", "activate", "deactivate"
    }

    public class BulkFileOperationResult
    {
        public bool Success { get; set; }
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
    }
}
