using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LMS.Web.DTOs
{
    public class UploadFileRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [StringLength(255)]
        public string? CustomFileName { get; set; }

        public bool IsPublic { get; set; } = false;

        [StringLength(500)]
        public string? Description { get; set; }

        public List<string>? Tags { get; set; }
    }

    public class BatchUploadRequest
    {
        [Required]
        public List<IFormFile> Files { get; set; } = new();

        public bool IsPublic { get; set; } = false;

        [StringLength(500)]
        public string? Description { get; set; }

        public List<string>? Tags { get; set; }
    }
}
