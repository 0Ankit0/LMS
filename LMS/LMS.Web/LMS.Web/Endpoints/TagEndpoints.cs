
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class TagEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags");
        group.MapGet("/", async (ITagRepository repo) => await repo.GetTagsAsync())
            .WithName("GetTags").WithSummary("Get all tags");
        group.MapPost("/paginated", async (PaginationRequest req, ITagRepository repo) => await repo.GetTagsPaginatedAsync(req))
            .WithName("GetTagsPaginated").WithSummary("Get tags with pagination");
        group.MapGet("/{id}", async (int id, ITagRepository repo) => await repo.GetTagByIdAsync(id))
            .WithName("GetTagById").WithSummary("Get tag by ID");
        group.MapPost("/", async (CreateTagRequest req, ITagRepository repo) => await repo.CreateTagAsync(req))
            .WithName("CreateTag").WithSummary("Create a new tag");
        group.MapPut("/{id}", async (int id, CreateTagRequest req, ITagRepository repo) => await repo.UpdateTagAsync(id, req))
            .WithName("UpdateTag").WithSummary("Update a tag");
        group.MapDelete("/{id}", async (int id, ITagRepository repo) => await repo.DeleteTagAsync(id))
            .WithName("DeleteTag").WithSummary("Delete a tag by ID");
    }
}
