using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Info, Warning, Success, Error

        [StringLength(100)]
        public string? Category { get; set; } // Course, Assessment, System, etc.

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        [StringLength(2000)]
        public string? Metadata { get; set; } // JSON string for additional data

        // Navigation properties
        public User? User { get; set; }
    }
}
