
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
        group.MapGet("/", async (ITagRepository repo) => await repo.GetTagsAsync());
        group.MapPost("/paginated", async (PaginationRequest req, ITagRepository repo) => await repo.GetTagsPaginatedAsync(req));
        group.MapGet("/{id}", async (int id, ITagRepository repo) => await repo.GetTagByIdAsync(id));
        group.MapPost("/", async (CreateTagRequest req, ITagRepository repo) => await repo.CreateTagAsync(req));
        group.MapPut("/{id}", async (int id, CreateTagRequest req, ITagRepository repo) => await repo.UpdateTagAsync(id, req));
        group.MapDelete("/{id}", async (int id, ITagRepository repo) => await repo.DeleteTagAsync(id));
    }
}
