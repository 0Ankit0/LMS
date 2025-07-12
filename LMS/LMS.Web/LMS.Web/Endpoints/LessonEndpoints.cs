
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
        group.MapGet("/", async (ILessonRepository repo) => await repo.GetLessonsAsync())
            .WithName("GetLessons").WithSummary("Get all lessons");
        group.MapPost("/paginated", async (PaginationRequest req, ILessonRepository repo) => await repo.GetLessonsPaginatedAsync(req))
            .WithName("GetLessonsPaginated").WithSummary("Get lessons with pagination");
        group.MapGet("/{id}", async (int id, ILessonRepository repo) => await repo.GetLessonByIdAsync(id))
            .WithName("GetLessonById").WithSummary("Get lesson by ID");
        group.MapGet("/module/{moduleId}", async (int moduleId, ILessonRepository repo) => await repo.GetLessonsByModuleIdAsync(moduleId))
            .WithName("GetLessonsByModuleId").WithSummary("Get lessons by module ID");
        group.MapPost("/", async (CreateLessonRequest req, ILessonRepository repo) => await repo.CreateLessonAsync(req))
            .WithName("CreateLesson").WithSummary("Create a new lesson");
        group.MapPut("/{id}", async (int id, CreateLessonRequest req, ILessonRepository repo) => await repo.UpdateLessonAsync(id, req))
            .WithName("UpdateLesson").WithSummary("Update a lesson");
        group.MapDelete("/{id}", async (int id, ILessonRepository repo) => await repo.DeleteLessonAsync(id))
            .WithName("DeleteLesson").WithSummary("Delete a lesson by ID");
        group.MapPut("/{lessonId}/order", async (int lessonId, int newOrder, ILessonRepository repo) => await repo.UpdateLessonOrderAsync(lessonId, newOrder))
            .WithName("UpdateLessonOrder").WithSummary("Update the order of a lesson");
    }
}
