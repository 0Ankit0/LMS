using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities
{
    /// <summary>
    /// Central library for all files uploaded by users
    /// </summary>
    public class UserFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OwnerUserId { get; set; } = string.Empty;

        [ForeignKey("OwnerUserId")]
        public virtual User Owner { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(1024)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [Range(1, long.MaxValue)]
        public long FileSize { get; set; }

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties - files referenced by other entities
        public virtual ICollection<Course> CourseThumbnails { get; set; } = new List<Course>();
        public virtual ICollection<User> UserProfilePictures { get; set; } = new List<User>();
        public virtual ICollection<LessonResource> LessonResourceFiles { get; set; } = new List<LessonResource>();
        public virtual ICollection<Category> CategoryIcons { get; set; } = new List<Category>();
    }
}
