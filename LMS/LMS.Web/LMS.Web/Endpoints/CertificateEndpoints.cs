
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
        group.MapGet("/", async (ICertificateRepository repo) => await repo.GetCertificatesAsync());
        group.MapPost("/paginated", async (PaginationRequest req, ICertificateRepository repo) => await repo.GetCertificatesPaginatedAsync(req));
        group.MapGet("/{id}", async (int id, ICertificateRepository repo) => await repo.GetCertificateByIdAsync(id));
        group.MapGet("/user/{userId}", async (string userId, ICertificateRepository repo) => await repo.GetCertificatesByUserIdAsync(userId));
        group.MapGet("/course/{courseId}/user/{userId}", async (int courseId, string userId, ICertificateRepository repo) => await repo.GetCertificateByCourseAndUserAsync(courseId, userId));
        group.MapPost("/", async (CreateCertificateRequest req, ICertificateRepository repo) => await repo.IssueCertificateAsync(req));
        group.MapPost("/revoke/{certificateId}", async (int certificateId, ICertificateRepository repo) => await repo.RevokeCertificateAsync(certificateId));
        group.MapGet("/validate/{certificateNumber}", async (string certificateNumber, ICertificateRepository repo) => await repo.ValidateCertificateAsync(certificateNumber));
        group.MapGet("/number/{certificateNumber}", async (string certificateNumber, ICertificateRepository repo) => await repo.GetCertificateByNumberAsync(certificateNumber));
    }
}
