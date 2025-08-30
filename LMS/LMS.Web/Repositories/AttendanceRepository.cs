using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using LMS.Data.DTOs;
using LMS.Web.Data;
using LMS.Data.DTOs.LMS;

namespace LMS.Repositories
{
    public interface IAttendanceRepository
    {
        Task<bool> IsInstructorOfCourseAsync(string userId, int courseId);
        Task<BatchAttendanceResultDto> SubmitBatchAttendanceAsync(BatchAttendanceDto batchAttendance, string submittedBy);
        Task<IEnumerable<AttendanceDto>> GetCourseAttendanceAsync(int courseId);
        Task<IEnumerable<AttendanceDto>> GetUserAttendanceAsync(string userId);
        Task<bool> CanUserUpdateAttendanceAsync(string userId, int attendanceId);
        Task<AttendanceDto?> UpdateAttendanceRecordAsync(int id, UpdateAttendanceDto updateAttendance, string updatedBy);
        Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(string userId);
        Task<bool> HasCourseAccessAsync(string userId, int courseId);
        Task<IEnumerable<CourseSessionDto>> GetCourseSessionsAsync(int courseId);
    }

    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly ApplicationDbContext _context;

        public AttendanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsInstructorOfCourseAsync(string userId, int courseId)
        {
            return await _context.Courses
                .AnyAsync(c => c.Id == courseId && c.InstructorId == userId);
        }

        public async Task<BatchAttendanceResultDto> SubmitBatchAttendanceAsync(BatchAttendanceDto batchAttendance, string submittedBy)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == batchAttendance.CourseId);

            if (course == null)
            {
                throw new ArgumentException("Course not found");
            }

            var attendanceRecords = new List<Attendance>();
            var errors = new List<string>();

            foreach (var record in batchAttendance.AttendanceRecords)
            {
                try
                {
                    var student = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == record.UserId);

                    if (student == null)
                    {
                        errors.Add($"Student with ID {record.UserId} not found");
                        continue;
                    }

                    // Parse status string to enum
                    if (!Enum.TryParse<AttendanceStatus>(record.Status, true, out var status))
                    {
                        status = AttendanceStatus.Present; // Default value
                    }

                    var attendance = new Attendance
                    {
                        StudentId = record.UserId,
                        ClassId = batchAttendance.CourseId,
                        Date = batchAttendance.SessionDate,
                        Status = status,
                        Notes = record.Notes,
                        CreatedBy = submittedBy,
                        CreatedAt = DateTime.UtcNow
                    };

                    attendanceRecords.Add(attendance);
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing record for student {record.UserId}: {ex.Message}");
                }
            }

            if (attendanceRecords.Any())
            {
                _context.Attendances.AddRange(attendanceRecords);
                await _context.SaveChangesAsync();
            }

            return new BatchAttendanceResultDto
            {
                SuccessCount = attendanceRecords.Count,
                ErrorCount = errors.Count,
                Errors = errors,
                ProcessedAt = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<AttendanceDto>> GetCourseAttendanceAsync(int courseId)
        {
            return await _context.Attendances
                .Where(a => a.ClassId == courseId)
                .Include(a => a.Class)
                .Include(a => a.Student)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId ?? string.Empty,
                    StudentName = a.Student != null ? a.Student.FirstName + " " + a.Student.LastName : string.Empty,
                    CourseId = a.ClassId,
                    CourseName = a.Class != null ? a.Class.Title : string.Empty,
                    Date = a.Date,
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    SubmittedBy = a.CreatedBy,
                    SubmittedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceDto>> GetUserAttendanceAsync(string userId)
        {
            return await _context.Attendances
                .Where(a => a.StudentId == userId)
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId ?? string.Empty,
                    StudentName = a.Student != null ? a.Student.FirstName + " " + a.Student.LastName : string.Empty,
                    CourseId = a.ClassId,
                    CourseName = a.Class != null ? a.Class.Title : string.Empty,
                    Date = a.Date,
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    SubmittedBy = a.CreatedBy,
                    SubmittedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> CanUserUpdateAttendanceAsync(string userId, int attendanceId)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Class)
                .FirstOrDefaultAsync(a => a.Id == attendanceId);

            if (attendance == null)
                return false;

            // User can update if they are the instructor of the course or created the attendance
            return attendance.Class?.InstructorId == userId || attendance.CreatedBy == userId;
        }

        public async Task<AttendanceDto?> UpdateAttendanceRecordAsync(int id, UpdateAttendanceDto updateAttendance, string updatedBy)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
                return null;

            // Parse status string to enum
            if (Enum.TryParse<AttendanceStatus>(updateAttendance.Status, true, out var status))
            {
                attendance.Status = status;
            }

            attendance.Notes = updateAttendance.Notes;
            attendance.UpdatedBy = updatedBy;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new AttendanceDto
            {
                Id = attendance.Id,
                StudentId = attendance.StudentId ?? string.Empty,
                StudentName = attendance.Student != null ? attendance.Student.FirstName + " " + attendance.Student.LastName : string.Empty,
                CourseId = attendance.ClassId,
                CourseName = attendance.Class != null ? attendance.Class.Title : string.Empty,
                Date = attendance.Date,
                Status = attendance.Status.ToString(),
                Notes = attendance.Notes,
                SubmittedBy = attendance.UpdatedBy,
                SubmittedAt = attendance.UpdatedAt
            };
        }

        public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(string userId)
        {
            var attendanceRecords = await _context.Attendances
                .Where(a => a.StudentId == userId)
                .ToListAsync();

            var totalSessions = attendanceRecords.Count;
            var presentCount = attendanceRecords.Count(a => a.Status == AttendanceStatus.Present);
            var absentCount = attendanceRecords.Count(a => a.Status == AttendanceStatus.Absent);
            var lateCount = attendanceRecords.Count(a => a.Status == AttendanceStatus.Late);
            var excusedCount = attendanceRecords.Count(a => a.Status == AttendanceStatus.Excused);

            return new AttendanceSummaryDto
            {
                UserId = userId,
                TotalSessions = totalSessions,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                LateCount = lateCount,
                ExcusedCount = excusedCount,
                AttendanceRate = totalSessions > 0 ? (double)presentCount / totalSessions * 100 : 0
            };
        }

        public async Task<bool> HasCourseAccessAsync(string userId, int courseId)
        {
            // Check if user is enrolled in the course or is the instructor
            var hasAccess = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (!hasAccess)
            {
                hasAccess = await _context.Courses
                    .AnyAsync(c => c.Id == courseId && c.InstructorId == userId);
            }

            return hasAccess;
        }

        public async Task<IEnumerable<CourseSessionDto>> GetCourseSessionsAsync(int courseId)
        {
            // Get distinct dates from attendance records for this course
            var sessions = await _context.Attendances
                .Where(a => a.ClassId == courseId)
                .GroupBy(a => a.Date)
                .Select(g => new CourseSessionDto
                {
                    CourseId = courseId,
                    Date = g.Key,
                    TotalStudents = g.Count(),
                    PresentCount = g.Count(a => a.Status == AttendanceStatus.Present),
                    AbsentCount = g.Count(a => a.Status == AttendanceStatus.Absent),
                    LateCount = g.Count(a => a.Status == AttendanceStatus.Late),
                    ExcusedCount = g.Count(a => a.Status == AttendanceStatus.Excused)
                })
                .OrderBy(s => s.Date)
                .ToListAsync();

            return sessions;
        }
    }
}
