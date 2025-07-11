
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class LessonEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lessons");
        group.MapGet("/", async (ILessonRepository repo) => await repo.GetLessonsAsync());
        group.MapPost("/paginated", async (PaginationRequest req, ILessonRepository repo) => await repo.GetLessonsPaginatedAsync(req));
        group.MapGet("/{id}", async (int id, ILessonRepository repo) => await repo.GetLessonByIdAsync(id));
        group.MapGet("/module/{moduleId}", async (int moduleId, ILessonRepository repo) => await repo.GetLessonsByModuleIdAsync(moduleId));
        group.MapPost("/", async (CreateLessonRequest req, ILessonRepository repo) => await repo.CreateLessonAsync(req));
        group.MapPut("/{id}", async (int id, CreateLessonRequest req, ILessonRepository repo) => await repo.UpdateLessonAsync(id, req));
        group.MapDelete("/{id}", async (int id, ILessonRepository repo) => await repo.DeleteLessonAsync(id));
        group.MapPut("/{lessonId}/order", async (int lessonId, int newOrder, ILessonRepository repo) => await repo.UpdateLessonOrderAsync(lessonId, newOrder));
    }
}
