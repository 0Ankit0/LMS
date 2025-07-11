
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class CategoryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories");
        group.MapGet("/", async (ICategoryRepository repo) => await repo.GetCategoriesAsync());
        group.MapPost("/paginated", async (PaginationRequest req, ICategoryRepository repo) => await repo.GetCategoriesPaginatedAsync(req));
        group.MapGet("/root", async (ICategoryRepository repo) => await repo.GetRootCategoriesAsync());
        group.MapGet("/{id}", async (int id, ICategoryRepository repo) => await repo.GetCategoryByIdAsync(id));
        group.MapGet("/{parentCategoryId}/subcategories", async (int parentCategoryId, ICategoryRepository repo) => await repo.GetSubCategoriesAsync(parentCategoryId));
        group.MapPost("/", async (CreateCategoryRequest req, ICategoryRepository repo) => await repo.CreateCategoryAsync(req));
        group.MapPut("/{id}", async (int id, CreateCategoryRequest req, ICategoryRepository repo) => await repo.UpdateCategoryAsync(id, req));
        group.MapDelete("/{id}", async (int id, ICategoryRepository repo) => await repo.DeleteCategoryAsync(id));
        group.MapGet("/course/{courseId}", async (int courseId, ICategoryRepository repo) => await repo.GetCategoriesByCourseIdAsync(courseId));
    }
}
