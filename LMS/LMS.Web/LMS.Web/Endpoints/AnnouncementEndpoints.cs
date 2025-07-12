
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
        group.MapGet("/", async (IAnnouncementRepository repo) => await repo.GetAnnouncementsAsync())
            .WithName("GetAnnouncements").WithSummary("Get all announcements");
        group.MapGet("/latest", async (IAnnouncementRepository repo) => await repo.GetLatestAnnouncementsAsync());
        group.MapGet("/course/{courseId}", async (int courseId, IAnnouncementRepository repo) => await repo.GetAnnouncementsByCourseAsync(courseId));
        group.MapGet("/{id}", async (int id, IAnnouncementRepository repo) => await repo.GetAnnouncementByIdAsync(id))
            .WithName("GetAnnouncementById").WithSummary("Get announcement by ID");
        group.MapPost("/", async (CreateAnnouncementRequest req, IAnnouncementRepository repo) => await repo.CreateAnnouncementAsync(req));
        group.MapPut("/{id}", async (int id, CreateAnnouncementRequest req, IAnnouncementRepository repo) => await repo.UpdateAnnouncementAsync(id, req))
            .WithName("UpdateAnnouncement").WithSummary("Update an announcement");
        group.MapDelete("/{id}", async (int id, IAnnouncementRepository repo) => await repo.DeleteAnnouncementAsync(id))
            .WithName("DeleteAnnouncement").WithSummary("Delete an announcement by ID");
        group.MapGet("/filter", async (string? searchTerm, string? priority, string? sortBy, IAnnouncementRepository repo) => await repo.GetFilteredAnnouncementsAsync(searchTerm, priority, sortBy));
    }
}
