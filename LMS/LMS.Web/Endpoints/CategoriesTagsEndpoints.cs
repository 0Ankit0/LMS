using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Infrastructure;
using LMS.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LMS.Web.Endpoints
{
    public class CategoriesTagsEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var categoriesGroup = app.MapGroup("/api/categories").WithTags("Categories");
            var tagsGroup = app.MapGroup("/api/tags").WithTags("Tags");

            // Categories endpoints
            categoriesGroup.MapGet("", GetCategories)
                .WithName("GetCategories")
                .WithSummary("Get hierarchy of all course categories");

            categoriesGroup.MapPost("", CreateCategory)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("CreateCategory")
                .WithSummary("Create a new category (Admin only)");

            categoriesGroup.MapPut("/{id:int}", UpdateCategory)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("UpdateCategory")
                .WithSummary("Update a category (Admin only)");

            categoriesGroup.MapDelete("/{id:int}", DeleteCategory)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("DeleteCategory")
                .WithSummary("Delete a category (Admin only)");

            // Tags endpoints
            tagsGroup.MapGet("", GetTags)
                .WithName("GetTags")
                .WithSummary("Get all tags with usage counts");

            tagsGroup.MapPost("", CreateTag)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("CreateTag")
                .WithSummary("Create a new tag (Admin only)");

            tagsGroup.MapPut("/{id:int}", UpdateTag)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("UpdateTag")
                .WithSummary("Update a tag (Admin only)");

            tagsGroup.MapDelete("/{id:int}", DeleteTag)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("DeleteTag")
                .WithSummary("Delete a tag (Admin only)");
        }

        // Categories methods
        private static async Task<IResult> GetCategories(ICategoryRepository categoryRepository)
        {
            try
            {
                var categories = await categoryRepository.GetCategoriesAsync();
                return Results.Ok(categories);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving categories: {ex.Message}");
            }
        }

        private static async Task<IResult> CreateCategory(
            CreateCategoryRequest createCategory,
            ICategoryRepository categoryRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var category = await categoryRepository.CreateCategoryAsync(createCategory);
                return Results.CreatedAtRoute("GetCategories", new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating category: {ex.Message}");
            }
        }

        private static async Task<IResult> UpdateCategory(
            int id,
            CreateCategoryRequest updateCategory,
            ICategoryRepository categoryRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var category = await categoryRepository.UpdateCategoryAsync(id, updateCategory);

                if (category == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(category);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating category: {ex.Message}");
            }
        }

        private static async Task<IResult> DeleteCategory(
            int id,
            ICategoryRepository categoryRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var success = await categoryRepository.DeleteCategoryAsync(id);

                if (!success)
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting category: {ex.Message}");
            }
        }

        // Tags methods
        private static async Task<IResult> GetTags(ITagRepository tagRepository)
        {
            try
            {
                var tags = await tagRepository.GetTagsAsync();
                return Results.Ok(tags);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving tags: {ex.Message}");
            }
        }

        private static async Task<IResult> CreateTag(
            CreateTagRequest createTag,
            ITagRepository tagRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var tag = await tagRepository.CreateTagAsync(createTag);
                return Results.CreatedAtRoute("GetTags", new { id = tag.Id }, tag);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating tag: {ex.Message}");
            }
        }

        private static async Task<IResult> UpdateTag(
            int id,
            CreateTagRequest updateTag,
            ITagRepository tagRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var tag = await tagRepository.UpdateTagAsync(id, updateTag);

                if (tag == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(tag);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating tag: {ex.Message}");
            }
        }

        private static async Task<IResult> DeleteTag(
            int id,
            ITagRepository tagRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var success = await tagRepository.DeleteTagAsync(id);

                if (!success)
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting tag: {ex.Message}");
            }
        }
    }
}
