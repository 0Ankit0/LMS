using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using LMS.Data.DTOs;
using LMS.Web.Data;

namespace LMS.Repositories
{
    public interface ICertificateRepository
    {
        Task<List<CertificateModel>> GetCertificatesAsync();
        Task<PaginatedResult<CertificateModel>> GetCertificatesPaginatedAsync(PaginationRequest request);
        Task<CertificateModel?> GetCertificateByIdAsync(int id);
        Task<List<CertificateModel>> GetCertificatesByUserIdAsync(string userId);
        Task<List<CertificateModel>> GetUserCertificatesAsync(string userId);
        Task<CertificateModel?> GetCertificateByCourseAndUserAsync(int courseId, string userId);
        Task<CertificateModel> IssueCertificateAsync(CreateCertificateRequest request);
        Task<bool> RevokeCertificateAsync(int certificateId);
        Task<bool> ValidateCertificateAsync(string certificateNumber);
        Task<CertificateModel?> GetCertificateByNumberAsync(string certificateNumber);
        Task<bool> UpdateCertificateAsync(int certificateId, CreateCertificateRequest request);
    }

    public class CertificateRepository : ICertificateRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<CertificateRepository> _logger;

        public CertificateRepository(ApplicationDbContext context, ILogger<CertificateRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<List<CertificateModel>> GetCertificatesAsync()
        {
            try
            {
                var certificates = await _context.Certificates
                    .Include(c => c.User)
                    .Include(c => c.Course)
                    .OrderByDescending(c => c.IssuedAt)
                    .ToListAsync();

                return certificates.Select(MapToCertificateModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting certificates");
                throw;
            }
        }





        public async Task<PaginatedResult<CertificateModel>> GetCertificatesPaginatedAsync(PaginationRequest request)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated certificates");
                throw;
            }
        }


        public async Task<CertificateModel?> GetCertificateByIdAsync(int id)
        {
            try
            {
                var certificate = await _context.Certificates
                    .Include(c => c.User)
                    .Include(c => c.Course)
                    .FirstOrDefaultAsync(c => c.Id == id);
                return certificate != null ? MapToCertificateModel(certificate) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting certificate by id: {Id}", id);
                throw;
            }
        }


        public async Task<List<CertificateModel>> GetCertificatesByUserIdAsync(string userId)
        {
            try
            {
                var certificates = await _context.Certificates
                    .Include(c => c.User)
                    .Include(c => c.Course)
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.IssuedAt)
                    .ToListAsync();
                return certificates.Select(MapToCertificateModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting certificates by user id: {UserId}", userId);
                throw;
            }
        }


        public async Task<List<CertificateModel>> GetUserCertificatesAsync(string userId)
        {
            // For compatibility, just call GetCertificatesByUserIdAsync
            return await GetCertificatesByUserIdAsync(userId);
        }


        public async Task<CertificateModel?> GetCertificateByCourseAndUserAsync(int courseId, string userId)
        {
            try
            {
                var certificate = await _context.Certificates
                    .Include(c => c.User)
                    .Include(c => c.Course)
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && c.UserId == userId);
                return certificate != null ? MapToCertificateModel(certificate) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting certificate by course and user: {CourseId}, {UserId}", courseId, userId);
                throw;
            }
        }












        public async Task<CertificateModel> IssueCertificateAsync(CreateCertificateRequest request)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing certificate for user: {UserId}, course: {CourseId}", request.UserId, request.CourseId);
                throw;
            }
        }





        public async Task<bool> RevokeCertificateAsync(int certificateId)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking certificate: {CertificateId}", certificateId);
                throw;
            }
        }




        public async Task<bool> ValidateCertificateAsync(string certificateNumber)
        {
            try
            {
                var certificate = await _context.Certificates
                    .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);
                if (certificate == null)
                    return false;

                // Check if certificate is valid and not expired
                var isValid = certificate.IsValid &&
                             (!certificate.ExpiresAt.HasValue || certificate.ExpiresAt.Value > DateTime.UtcNow);
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating certificate: {CertificateNumber}", certificateNumber);
                throw;
            }
        }


        public async Task<CertificateModel?> GetCertificateByNumberAsync(string certificateNumber)
        {
            try
            {
                var certificate = await _context.Certificates
                    .Include(c => c.User)
                    .Include(c => c.Course)
                    .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);
                return certificate != null ? MapToCertificateModel(certificate) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting certificate by number: {CertificateNumber}", certificateNumber);
                throw;
            }
        }




        private async Task<string> GenerateCertificateNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"CERT-{year}-";
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

        public async Task<bool> UpdateCertificateAsync(int certificateId, CreateCertificateRequest request)
        {
            try
            {
                var certificate = await _context.Certificates.FindAsync(certificateId);
                if (certificate == null)
                    return false;

                certificate.UserId = request.UserId;
                certificate.CourseId = request.CourseId;
                certificate.FinalGrade = request.FinalGrade;
                certificate.ExpiresAt = request.ExpiresAt;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating certificate: {CertificateId}", certificateId);
                throw;
            }
        }
    }
}
