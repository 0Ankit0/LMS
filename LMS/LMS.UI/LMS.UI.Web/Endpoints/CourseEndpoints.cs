
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
        group.MapGet("/", async (ICourseRepository repo) => await repo.GetCoursesAsync())
            .WithName("GetCourses").WithSummary("Get all courses");
        group.MapPost("/paginated", async (PaginationRequest req, ICourseRepository repo) => await repo.GetCoursesPaginatedAsync(req))
            .WithName("GetCoursesPaginated").WithSummary("Get courses with pagination");
        group.MapGet("/{id}", async (int id, ICourseRepository repo) => await repo.GetCourseByIdAsync(id))
            .WithName("GetCourseById").WithSummary("Get course by ID");
        group.MapPost("/", async (CreateCourseRequest req, ICourseRepository repo) => await repo.CreateCourseAsync(req))
            .WithName("CreateCourse").WithSummary("Create a new course");
        group.MapPut("/{id}", async (int id, CreateCourseRequest req, ICourseRepository repo) => await repo.UpdateCourseAsync(id, req))
            .WithName("UpdateCourse").WithSummary("Update a course");
        group.MapDelete("/{id}", async (int id, ICourseRepository repo) => await repo.DeleteCourseAsync(id))
            .WithName("DeleteCourse").WithSummary("Delete a course by ID");
        group.MapGet("/{courseId}/modules", async (int courseId, ICourseRepository repo) => await repo.GetCourseModulesAsync(courseId))
            .WithName("GetCourseModules").WithSummary("Get modules for a course");
        group.MapPost("/{courseId}/modules", async (int courseId, CreateModuleRequest req, ICourseRepository repo) => await repo.CreateModuleAsync(req))
            .WithName("CreateModuleForCourse").WithSummary("Create a module for a course");
        group.MapGet("/modules/{moduleId}/lessons", async (int moduleId, ICourseRepository repo) => await repo.GetModuleLessonsAsync(moduleId))
            .WithName("GetModuleLessons").WithSummary("Get lessons for a module");
        group.MapPost("/modules/{moduleId}/lessons", async (int moduleId, CreateLessonRequest req, ICourseRepository repo) => await repo.CreateLessonAsync(req))
            .WithName("CreateLessonForModule").WithSummary("Create a lesson for a module");
    }
}
