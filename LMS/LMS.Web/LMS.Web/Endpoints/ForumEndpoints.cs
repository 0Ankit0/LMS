
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class ForumEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/forums");
        group.MapGet("/", async (IForumRepository repo) => await repo.GetForumsAsync());
        group.MapPost("/paginated", async (PaginationRequest req, IForumRepository repo) => await repo.GetForumsPaginatedAsync(req));
        group.MapGet("/{id}", async (int id, IForumRepository repo) => await repo.GetForumByIdAsync(id));
        group.MapGet("/course/{courseId}", async (int courseId, IForumRepository repo) => await repo.GetForumsByCourseIdAsync(courseId));
        group.MapPost("/", async (CreateForumRequest req, IForumRepository repo) => await repo.CreateForumAsync(req));
        group.MapPut("/{id}", async (int id, CreateForumRequest req, IForumRepository repo) => await repo.UpdateForumAsync(id, req));
        group.MapDelete("/{id}", async (int id, IForumRepository repo) => await repo.DeleteForumAsync(id));
        group.MapGet("/{forumId}/topics", async (int forumId, IForumRepository repo) => await repo.GetTopicsByForumIdAsync(forumId));
        group.MapGet("/topics/{id}", async (int id, IForumRepository repo) => await repo.GetTopicByIdAsync(id));
        group.MapPost("/topics", async (CreateForumTopicRequest req, string userId, IForumRepository repo) => await repo.CreateTopicAsync(req, userId));
        group.MapDelete("/topics/{id}", async (int id, IForumRepository repo) => await repo.DeleteTopicAsync(id));
        group.MapGet("/topics/{topicId}/posts", async (int topicId, IForumRepository repo) => await repo.GetPostsByTopicIdAsync(topicId));
        group.MapGet("/posts", async (IForumRepository repo) => await repo.GetAllForumPostsAsync());
        group.MapPost("/posts/paginated", async (PaginationRequest req, IForumRepository repo) => await repo.GetAllForumPostsPaginatedAsync(req));
        group.MapGet("/posts/{id}", async (int id, IForumRepository repo) => await repo.GetPostByIdAsync(id));
        group.MapPost("/posts", async (CreateForumPostRequest req, string authorId, IForumRepository repo) => await repo.CreatePostAsync(req, authorId));
        group.MapDelete("/posts/{id}", async (int id, IForumRepository repo) => await repo.DeletePostAsync(id));
    }
}
