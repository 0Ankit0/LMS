using LMS.Data.Entities;
using LMS.Models.User;
using LMS.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LMS.Services
{
    public interface ICertificateService
    {
        Task<List<CertificateModel>> GetCertificatesAsync();
        Task<PaginatedResult<CertificateModel>> GetCertificatesPaginatedAsync(PaginationRequest request);
        Task<CertificateModel?> GetCertificateByIdAsync(int id);
        Task<List<CertificateModel>> GetCertificatesByUserIdAsync(string userId);
        Task<CertificateModel?> GetCertificateByCourseAndUserAsync(int courseId, string userId);
        Task<CertificateModel> IssueCertificateAsync(CreateCertificateRequest request);
        Task<bool> RevokeCertificateAsync(int certificateId);
        Task<bool> ValidateCertificateAsync(string certificateNumber);
        Task<CertificateModel?> GetCertificateByNumberAsync(string certificateNumber);
    }

    public class CertificateService : ICertificateService
    {
        private readonly IDbContextFactory<AuthDbContext> _contextFactory;

        public CertificateService(IDbContextFactory<AuthDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<CertificateModel>> GetCertificatesAsync()
        {
            await using var _context = _contextFactory.CreateDbContext();
            var certificates = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .OrderByDescending(c => c.IssuedAt)
                .ToListAsync();

            return certificates.Select(MapToCertificateModel).ToList();
        }

        public async Task<PaginatedResult<CertificateModel>> GetCertificatesPaginatedAsync(PaginationRequest request)
        {
            await using var _context = _contextFactory.CreateDbContext();

            var query = _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .OrderByDescending(c => c.IssuedAt);

            var totalCount = await query.CountAsync();

            var certificates = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginatedResult<CertificateModel>
            {
                Items = certificates.Select(MapToCertificateModel).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<CertificateModel?> GetCertificateByIdAsync(int id)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var certificate = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == id);

            return certificate != null ? MapToCertificateModel(certificate) : null;
        }

        public async Task<List<CertificateModel>> GetCertificatesByUserIdAsync(string userId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var certificates = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IssuedAt)
                .ToListAsync();

            return certificates.Select(MapToCertificateModel).ToList();
        }

        public async Task<CertificateModel?> GetCertificateByCourseAndUserAsync(int courseId, string userId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var certificate = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.UserId == userId);

            return certificate != null ? MapToCertificateModel(certificate) : null;
        }

        public async Task<CertificateModel> IssueCertificateAsync(CreateCertificateRequest request)
        {
            await using var _context = _contextFactory.CreateDbContext();

            // Check if certificate already exists
            var existingCertificate = await _context.Certificates
                .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.CourseId == request.CourseId);

            if (existingCertificate != null)
                throw new InvalidOperationException("Certificate already issued for this user and course");

            // Verify enrollment and completion
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == request.UserId && e.CourseId == request.CourseId);

            if (enrollment == null)
                throw new InvalidOperationException("User is not enrolled in this course");

            if (enrollment.Status != EnrollmentStatus.Completed)
                throw new InvalidOperationException("Course must be completed before issuing certificate");

            // Generate certificate number
            var certificateNumber = await GenerateCertificateNumberAsync();

            var certificate = new Certificate
            {
                UserId = request.UserId,
                CourseId = request.CourseId,
                CertificateNumber = certificateNumber,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt,
                FinalGrade = request.FinalGrade,
                IsValid = true
            };

            _context.Certificates.Add(certificate);

            // Update enrollment certificate status
            enrollment.IsCertificateIssued = true;
            enrollment.CertificateIssuedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCertificateByIdAsync(certificate.Id) ?? throw new InvalidOperationException("Failed to retrieve issued certificate");
        }

        public async Task<bool> RevokeCertificateAsync(int certificateId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var certificate = await _context.Certificates.FindAsync(certificateId);
            if (certificate == null)
                return false;

            certificate.IsValid = false;

            // Update enrollment status
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == certificate.UserId && e.CourseId == certificate.CourseId);

            if (enrollment != null)
            {
                enrollment.IsCertificateIssued = false;
                enrollment.CertificateIssuedAt = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateCertificateAsync(string certificateNumber)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var certificate = await _context.Certificates
                .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);

            if (certificate == null)
                return false;

            // Check if certificate is valid and not expired
            var isValid = certificate.IsValid &&
                         (!certificate.ExpiresAt.HasValue || certificate.ExpiresAt.Value > DateTime.UtcNow);

            return isValid;
        }

        public async Task<CertificateModel?> GetCertificateByNumberAsync(string certificateNumber)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var certificate = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);

            return certificate != null ? MapToCertificateModel(certificate) : null;
        }

        private async Task<string> GenerateCertificateNumberAsync()
        {
            await using var _context = _contextFactory.CreateDbContext();
            var year = DateTime.UtcNow.Year;
            var prefix = $"CERT-{year}-";

            // Get the last certificate number for this year
            var lastCertificate = await _context.Certificates
                .Where(c => c.CertificateNumber.StartsWith(prefix))
                .OrderByDescending(c => c.CertificateNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastCertificate != null)
            {
                var lastNumberPart = lastCertificate.CertificateNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D6}"; // 6-digit padded number
        }

        private static CertificateModel MapToCertificateModel(Certificate certificate)
        {
            return new CertificateModel
            {
                Id = certificate.Id,
                UserId = certificate.UserId,
                UserName = certificate.User?.UserName ?? "",
                UserEmail = certificate.User?.Email ?? "",
                CourseId = certificate.CourseId,
                CourseTitle = certificate.Course?.Title ?? "",
                CertificateNumber = certificate.CertificateNumber,
                IssuedAt = certificate.IssuedAt,
                ExpiresAt = certificate.ExpiresAt,
                FinalGrade = certificate.FinalGrade,
                CertificateUrl = certificate.CertificateUrl,
                IsValid = certificate.IsValid
            };
        }
    }
}
