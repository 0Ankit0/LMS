using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using LMS.Data.DTOs;
using LMS.Web.Data;
using Microsoft.Extensions.Logging;

namespace LMS.Repositories
{
    public interface IEnrollmentRepository
    {
        Task<List<EnrollmentModel>> GetEnrollmentsAsync();
        Task<PaginatedResult<EnrollmentModel>> GetEnrollmentsPaginatedAsync(PaginationRequest request);
        Task<EnrollmentModel?> GetEnrollmentByIdAsync(int id);
        Task<List<EnrollmentModel>> GetEnrollmentsByUserIdAsync(string userId);
        Task<List<EnrollmentModel>> GetUserEnrollmentsAsync(string userId); // Add this method
        Task<List<EnrollmentModel>> GetEnrollmentsByCourseIdAsync(int courseId);
        Task<EnrollmentModel?> GetEnrollmentByUserAndCourseAsync(string userId, int courseId);
        Task<EnrollmentModel> CreateEnrollmentAsync(string userId, CreateEnrollmentRequest request);
        Task<EnrollmentModel> UpdateEnrollmentStatusAsync(int id, string status);
        Task<bool> DeleteEnrollmentAsync(int id);
        Task<bool> UnenrollUserAsync(string userId, int courseId); // Add this method
        Task<bool> IsUserEnrolledInCourseAsync(string userId, int courseId);
        Task<int> GetCourseEnrollmentCountAsync(int courseId);
        Task<bool> EnrollUserAsync(string userId, int courseId);
    }

    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnrollmentRepository> _logger;

        public EnrollmentRepository(ApplicationDbContext context, ILogger<EnrollmentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<EnrollmentModel>> GetEnrollmentsAsync()
        {
            try
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrolledAt)
                    .ToListAsync();

                return enrollments.Select(MapToEnrollmentModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching enrollments");
                throw;
            }
        }

        public async Task<PaginatedResult<EnrollmentModel>> GetEnrollmentsPaginatedAsync(PaginationRequest request)
        {
            var query = _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrolledAt);

            var totalCount = await query.CountAsync();

            var enrollments = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginatedResult<EnrollmentModel>
            {
                Items = enrollments.Select(MapToEnrollmentModel).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<EnrollmentModel?> GetEnrollmentByIdAsync(int id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id);

            return enrollment != null ? MapToEnrollmentModel(enrollment) : null;
        }

        public async Task<List<EnrollmentModel>> GetEnrollmentsByUserIdAsync(string userId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            return enrollments.Select(MapToEnrollmentModel).ToList();
        }

        public async Task<List<EnrollmentModel>> GetUserEnrollmentsAsync(string userId)
        {
            // This is an alias for GetEnrollmentsByUserIdAsync to match the interface
            return await GetEnrollmentsByUserIdAsync(userId);
        }

        public async Task<List<EnrollmentModel>> GetEnrollmentsByCourseIdAsync(int courseId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Where(e => e.CourseId == courseId)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            return enrollments.Select(MapToEnrollmentModel).ToList();
        }

        public async Task<EnrollmentModel?> GetEnrollmentByUserAndCourseAsync(string userId, int courseId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            return enrollment != null ? MapToEnrollmentModel(enrollment) : null;
        }

        public async Task<EnrollmentModel> CreateEnrollmentAsync(string userId, CreateEnrollmentRequest request)
        {
            // Check if user is already enrolled
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == request.CourseId);

            if (existingEnrollment != null)
                throw new InvalidOperationException("User is already enrolled in this course");

            // Check course capacity
            var course = await _context.Courses.FindAsync(request.CourseId);
            if (course == null)
                throw new ArgumentException("Course not found", nameof(request.CourseId));

            if (course.MaxEnrollments > 0)
            {
                var currentEnrollments = await _context.Enrollments
                    .CountAsync(e => e.CourseId == request.CourseId && e.Status == EnrollmentStatus.Active);

                if (currentEnrollments >= course.MaxEnrollments)
                    throw new InvalidOperationException("Course has reached maximum enrollment capacity");
            }

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = request.CourseId,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Active,
                ProgressPercentage = 0,
                TimeSpent = TimeSpan.Zero
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return await GetEnrollmentByIdAsync(enrollment.Id) ?? throw new InvalidOperationException("Failed to retrieve created enrollment");
        }

        public async Task<EnrollmentModel> UpdateEnrollmentStatusAsync(int id, string status)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                throw new ArgumentException("Enrollment not found", nameof(id));

            if (!Enum.TryParse<EnrollmentStatus>(status, out var enrollmentStatus))
                throw new ArgumentException("Invalid enrollment status", nameof(status));

            enrollment.Status = enrollmentStatus;

            if (enrollmentStatus == EnrollmentStatus.Completed)
                enrollment.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetEnrollmentByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated enrollment");
        }

        public async Task<bool> DeleteEnrollmentAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return false;

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnenrollUserAsync(string userId, int courseId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment == null)
                return false;

            // Mark as dropped instead of deleting
            enrollment.Status = EnrollmentStatus.Dropped;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserEnrolledInCourseAsync(string userId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
        }

        public async Task<int> GetCourseEnrollmentCountAsync(int courseId)
        {
            return await _context.Enrollments
                .CountAsync(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
        }

        public async Task<bool> EnrollUserAsync(string userId, int courseId)
        {
            // Check if user is already enrolled
            var alreadyEnrolled = await IsUserEnrolledInCourseAsync(userId, courseId);
            if (alreadyEnrolled)
                return false;

            // Check course capacity
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return false;

            if (course.MaxEnrollments > 0)
            {
                var currentEnrollments = await _context.Enrollments.CountAsync(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
                if (currentEnrollments >= course.MaxEnrollments)
                    return false;
            }

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Active,
                ProgressPercentage = 0,
                TimeSpent = TimeSpan.Zero
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        private static EnrollmentModel MapToEnrollmentModel(Enrollment enrollment)
        {
            return new EnrollmentModel
            {
                Id = enrollment.Id,
                UserId = enrollment.UserId,
                UserName = enrollment.User?.UserName ?? "",
                CourseId = enrollment.CourseId,
                CourseTitle = enrollment.Course?.Title ?? "",
                CourseThumbnailUrl = enrollment.Course?.ThumbnailUrl ?? "",
                EnrolledAt = enrollment.EnrolledAt,
                StartedAt = enrollment.StartedAt,
                CompletedAt = enrollment.CompletedAt,
                Status = enrollment.Status.ToString(),
                ProgressPercentage = enrollment.ProgressPercentage,
                TimeSpent = enrollment.TimeSpent,
                FinalGrade = enrollment.FinalGrade,
                IsCertificateIssued = enrollment.IsCertificateIssued,
                CertificateIssuedAt = enrollment.CertificateIssuedAt
            };
        }
    }
}
