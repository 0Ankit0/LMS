
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class CertificateEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/certificates");
        group.MapGet("/", async (ICertificateRepository repo) => await repo.GetCertificatesAsync())
            .WithName("GetCertificates").WithSummary("Get all certificates");
        group.MapPost("/paginated", async (PaginationRequest req, ICertificateRepository repo) => await repo.GetCertificatesPaginatedAsync(req))
            .WithName("GetCertificatesPaginated").WithSummary("Get certificates with pagination");
        group.MapGet("/{id}", async (int id, ICertificateRepository repo) => await repo.GetCertificateByIdAsync(id))
            .WithName("GetCertificateById").WithSummary("Get certificate by ID");
        group.MapGet("/user/{userId}", async (string userId, ICertificateRepository repo) => await repo.GetCertificatesByUserIdAsync(userId))
            .WithName("GetCertificatesByUserId").WithSummary("Get certificates by user ID");
        group.MapGet("/course/{courseId}/user/{userId}", async (int courseId, string userId, ICertificateRepository repo) => await repo.GetCertificateByCourseAndUserAsync(courseId, userId))
            .WithName("GetCertificateByCourseAndUser").WithSummary("Get certificate by course and user");
        group.MapPost("/", async (CreateCertificateRequest req, ICertificateRepository repo) => await repo.IssueCertificateAsync(req))
            .WithName("IssueCertificate").WithSummary("Issue a new certificate");
        group.MapPost("/revoke/{certificateId}", async (int certificateId, ICertificateRepository repo) => await repo.RevokeCertificateAsync(certificateId))
            .WithName("RevokeCertificate").WithSummary("Revoke a certificate by ID");
        group.MapGet("/validate/{certificateNumber}", async (string certificateNumber, ICertificateRepository repo) => await repo.ValidateCertificateAsync(certificateNumber))
            .WithName("ValidateCertificate").WithSummary("Validate a certificate by number");
        group.MapGet("/number/{certificateNumber}", async (string certificateNumber, ICertificateRepository repo) => await repo.GetCertificateByNumberAsync(certificateNumber))
            .WithName("GetCertificateByNumber").WithSummary("Get certificate by certificate number");
    }
}
