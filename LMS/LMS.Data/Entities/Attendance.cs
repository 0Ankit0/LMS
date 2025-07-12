using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.Data.Entities
{
    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        Late = 3,
        Excused = 4,
        PartiallyPresent = 5,
        Sick = 6,
        OnLeave = 7
    }

    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        public int ClassId { get; set; } // Course or class reference

        public string? StudentId { get; set; } // Nullable for instructor attendance

        public string? InstructorId { get; set; } // Nullable for student attendance

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        public TimeSpan? CheckInTime { get; set; }

        public TimeSpan? CheckOutTime { get; set; }

        public TimeSpan? Duration { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

        // Additional fields for tracking
        public bool IsActive { get; set; } = true;

        public string? Reason { get; set; } // Reason for absence/lateness

        public bool IsExcused { get; set; } = false;

        // Navigation properties
        public Course? Class { get; set; }
        public User? Student { get; set; }
        public User? Instructor { get; set; }
    }
}
