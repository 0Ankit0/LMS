
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class AnnouncementEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/announcements");
        group.MapGet("/", async (IAnnouncementRepository repo) => await repo.GetAnnouncementsAsync());
        group.MapGet("/latest", async (IAnnouncementRepository repo) => await repo.GetLatestAnnouncementsAsync());
        group.MapGet("/course/{courseId}", async (int courseId, IAnnouncementRepository repo) => await repo.GetAnnouncementsByCourseAsync(courseId));
        group.MapGet("/{id}", async (int id, IAnnouncementRepository repo) => await repo.GetAnnouncementByIdAsync(id));
        group.MapPost("/", async (CreateAnnouncementRequest req, IAnnouncementRepository repo) => await repo.CreateAnnouncementAsync(req));
        group.MapPut("/{id}", async (int id, CreateAnnouncementRequest req, IAnnouncementRepository repo) => await repo.UpdateAnnouncementAsync(id, req));
        group.MapDelete("/{id}", async (int id, IAnnouncementRepository repo) => await repo.DeleteAnnouncementAsync(id));
        group.MapGet("/filter", async (string? searchTerm, string? priority, string? sortBy, IAnnouncementRepository repo) => await repo.GetFilteredAnnouncementsAsync(searchTerm, priority, sortBy));
    }
}
