
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class CourseEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses");
        group.MapGet("/", async (ICourseRepository repo) => await repo.GetCoursesAsync());
        group.MapPost("/paginated", async (PaginationRequest req, ICourseRepository repo) => await repo.GetCoursesPaginatedAsync(req));
        group.MapGet("/{id}", async (int id, ICourseRepository repo) => await repo.GetCourseByIdAsync(id));
        group.MapPost("/", async (CreateCourseRequest req, ICourseRepository repo) => await repo.CreateCourseAsync(req));
        group.MapPut("/{id}", async (int id, CreateCourseRequest req, ICourseRepository repo) => await repo.UpdateCourseAsync(id, req));
        group.MapDelete("/{id}", async (int id, ICourseRepository repo) => await repo.DeleteCourseAsync(id));
        group.MapGet("/{courseId}/modules", async (int courseId, ICourseRepository repo) => await repo.GetCourseModulesAsync(courseId));
        group.MapPost("/{courseId}/modules", async (int courseId, CreateModuleRequest req, ICourseRepository repo) => await repo.CreateModuleAsync(req));
        group.MapGet("/modules/{moduleId}/lessons", async (int moduleId, ICourseRepository repo) => await repo.GetModuleLessonsAsync(moduleId));
        group.MapPost("/modules/{moduleId}/lessons", async (int moduleId, CreateLessonRequest req, ICourseRepository repo) => await repo.CreateLessonAsync(req));
    }
}
