
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
        group.MapGet("/categories", async (string? search, int? take, IDropdownRepository repo) => await repo.GetCategoriesAsync(search, take ?? 20))
            .WithName("GetCategoriesDropdown").WithSummary("Get categories for dropdown");
        group.MapGet("/courses", async (string? search, int? take, IDropdownRepository repo) => await repo.GetCoursesAsync(search, take ?? 20))
            .WithName("GetCoursesDropdown").WithSummary("Get courses for dropdown");
        group.MapGet("/modules", async (string? search, int? take, IDropdownRepository repo) => await repo.GetModulesAsync(search, take ?? 20))
            .WithName("GetModulesDropdown").WithSummary("Get modules for dropdown");
        group.MapGet("/modules/by-course/{courseId}", async (int courseId, string? search, int? take, IDropdownRepository repo) => await repo.GetModulesByCourseAsync(courseId, search, take ?? 20))
            .WithName("GetModulesByCourseDropdown").WithSummary("Get modules by course for dropdown");
        group.MapGet("/users", async (string? search, int? take, IDropdownRepository repo) => await repo.GetUsersAsync(search, take ?? 20))
            .WithName("GetUsersDropdown").WithSummary("Get users for dropdown");
        group.MapGet("/instructors", async (string? search, int? take, IDropdownRepository repo) => await repo.GetInstructorsAsync(search, take ?? 20))
            .WithName("GetInstructorsDropdown").WithSummary("Get instructors for dropdown");
        group.MapGet("/tags", async (string? search, int? take, IDropdownRepository repo) => await repo.GetTagsAsync(search, take ?? 20))
            .WithName("GetTagsDropdown").WithSummary("Get tags for dropdown");
        group.MapGet("/forums", async (string? search, int? take, IDropdownRepository repo) => await repo.GetForumsAsync(search, take ?? 20))
            .WithName("GetForumsDropdown").WithSummary("Get forums for dropdown");
        group.MapGet("/assessments", async (string? search, int? take, IDropdownRepository repo) => await repo.GetAssessmentsAsync(search, take ?? 20))
            .WithName("GetAssessmentsDropdown").WithSummary("Get assessments for dropdown");
        group.MapGet("/lessons", async (string? search, int? take, IDropdownRepository repo) => await repo.GetLessonsAsync(search, take ?? 20))
            .WithName("GetLessonsDropdown").WithSummary("Get lessons for dropdown");
        group.MapGet("/lessons/by-module/{moduleId}", async (int moduleId, string? search, int? take, IDropdownRepository repo) => await repo.GetLessonsByModuleAsync(moduleId, search, take ?? 20))
            .WithName("GetLessonsByModuleDropdown").WithSummary("Get lessons by module for dropdown");
    }
}
