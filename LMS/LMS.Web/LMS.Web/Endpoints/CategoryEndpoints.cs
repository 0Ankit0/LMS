
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
        group.MapGet("/", async (ICategoryRepository repo) => await repo.GetCategoriesAsync())
            .WithName("GetCategories").WithSummary("Get all categories");
        group.MapPost("/paginated", async (PaginationRequest req, ICategoryRepository repo) => await repo.GetCategoriesPaginatedAsync(req))
            .WithName("GetCategoriesPaginated").WithSummary("Get categories with pagination");
        group.MapGet("/root", async (ICategoryRepository repo) => await repo.GetRootCategoriesAsync())
            .WithName("GetRootCategories").WithSummary("Get root categories");
        group.MapGet("/{id}", async (int id, ICategoryRepository repo) => await repo.GetCategoryByIdAsync(id))
            .WithName("GetCategoryById").WithSummary("Get category by ID");
        group.MapGet("/{parentCategoryId}/subcategories", async (int parentCategoryId, ICategoryRepository repo) => await repo.GetSubCategoriesAsync(parentCategoryId))
            .WithName("GetSubCategories").WithSummary("Get subcategories by parent category ID");
        group.MapPost("/", async (CreateCategoryRequest req, ICategoryRepository repo) => await repo.CreateCategoryAsync(req))
            .WithName("CreateCategory").WithSummary("Create a new category");
        group.MapPut("/{id}", async (int id, CreateCategoryRequest req, ICategoryRepository repo) => await repo.UpdateCategoryAsync(id, req))
            .WithName("UpdateCategory").WithSummary("Update a category");
        group.MapDelete("/{id}", async (int id, ICategoryRepository repo) => await repo.DeleteCategoryAsync(id))
            .WithName("DeleteCategory").WithSummary("Delete a category by ID");
        group.MapGet("/course/{courseId}", async (int courseId, ICategoryRepository repo) => await repo.GetCategoriesByCourseIdAsync(courseId))
            .WithName("GetCategoriesByCourseId").WithSummary("Get categories by course ID");
    }
}
