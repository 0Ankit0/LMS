
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class DropdownEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dropdowns");
        group.MapGet("/categories", async (string? search, int? take, IDropdownRepository repo) => await repo.GetCategoriesAsync(search, take ?? 20));
        group.MapGet("/courses", async (string? search, int? take, IDropdownRepository repo) => await repo.GetCoursesAsync(search, take ?? 20));
        group.MapGet("/modules", async (string? search, int? take, IDropdownRepository repo) => await repo.GetModulesAsync(search, take ?? 20));
        group.MapGet("/modules/by-course/{courseId}", async (int courseId, string? search, int? take, IDropdownRepository repo) => await repo.GetModulesByCourseAsync(courseId, search, take ?? 20));
        group.MapGet("/users", async (string? search, int? take, IDropdownRepository repo) => await repo.GetUsersAsync(search, take ?? 20));
        group.MapGet("/instructors", async (string? search, int? take, IDropdownRepository repo) => await repo.GetInstructorsAsync(search, take ?? 20));
        group.MapGet("/tags", async (string? search, int? take, IDropdownRepository repo) => await repo.GetTagsAsync(search, take ?? 20));
        group.MapGet("/forums", async (string? search, int? take, IDropdownRepository repo) => await repo.GetForumsAsync(search, take ?? 20));
        group.MapGet("/assessments", async (string? search, int? take, IDropdownRepository repo) => await repo.GetAssessmentsAsync(search, take ?? 20));
        group.MapGet("/lessons", async (string? search, int? take, IDropdownRepository repo) => await repo.GetLessonsAsync(search, take ?? 20));
        group.MapGet("/lessons/by-module/{moduleId}", async (int moduleId, string? search, int? take, IDropdownRepository repo) => await repo.GetLessonsByModuleAsync(moduleId, search, take ?? 20));
    }
}
