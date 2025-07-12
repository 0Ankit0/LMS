
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
        group.MapGet("/", async (IForumRepository repo) => await repo.GetForumsAsync())
            .WithName("GetForums").WithSummary("Get all forums");
        group.MapPost("/paginated", async (PaginationRequest req, IForumRepository repo) => await repo.GetForumsPaginatedAsync(req))
            .WithName("GetForumsPaginated").WithSummary("Get forums with pagination");
        group.MapGet("/{id}", async (int id, IForumRepository repo) => await repo.GetForumByIdAsync(id))
            .WithName("GetForumById").WithSummary("Get forum by ID");
        group.MapGet("/course/{courseId}", async (int courseId, IForumRepository repo) => await repo.GetForumsByCourseIdAsync(courseId))
            .WithName("GetForumsByCourseId").WithSummary("Get forums by course ID");
        group.MapPost("/", async (CreateForumRequest req, IForumRepository repo) => await repo.CreateForumAsync(req))
            .WithName("CreateForum").WithSummary("Create a new forum");
        group.MapPut("/{id}", async (int id, CreateForumRequest req, IForumRepository repo) => await repo.UpdateForumAsync(id, req))
            .WithName("UpdateForum").WithSummary("Update a forum");
        group.MapDelete("/{id}", async (int id, IForumRepository repo) => await repo.DeleteForumAsync(id))
            .WithName("DeleteForum").WithSummary("Delete a forum by ID");
        group.MapGet("/{forumId}/topics", async (int forumId, IForumRepository repo) => await repo.GetTopicsByForumIdAsync(forumId))
            .WithName("GetTopicsByForumId").WithSummary("Get topics by forum ID");
        group.MapGet("/topics/{id}", async (int id, IForumRepository repo) => await repo.GetTopicByIdAsync(id))
            .WithName("GetTopicById").WithSummary("Get topic by ID");
        group.MapPost("/topics", async (CreateForumTopicRequest req, string userId, IForumRepository repo) => await repo.CreateTopicAsync(req, userId))
            .WithName("CreateTopic").WithSummary("Create a new topic");
        group.MapDelete("/topics/{id}", async (int id, IForumRepository repo) => await repo.DeleteTopicAsync(id))
            .WithName("DeleteTopic").WithSummary("Delete a topic by ID");
        group.MapGet("/topics/{topicId}/posts", async (int topicId, IForumRepository repo) => await repo.GetPostsByTopicIdAsync(topicId))
            .WithName("GetPostsByTopicId").WithSummary("Get posts by topic ID");
        group.MapGet("/posts", async (IForumRepository repo) => await repo.GetAllForumPostsAsync())
            .WithName("GetAllForumPosts").WithSummary("Get all forum posts");
        group.MapPost("/posts/paginated", async (PaginationRequest req, IForumRepository repo) => await repo.GetAllForumPostsPaginatedAsync(req))
            .WithName("GetAllForumPostsPaginated").WithSummary("Get all forum posts with pagination");
        group.MapGet("/posts/{id}", async (int id, IForumRepository repo) => await repo.GetPostByIdAsync(id))
            .WithName("GetPostById").WithSummary("Get post by ID");
        group.MapPost("/posts", async (CreateForumPostRequest req, string authorId, IForumRepository repo) => await repo.CreatePostAsync(req, authorId))
            .WithName("CreatePost").WithSummary("Create a new post");
        group.MapDelete("/posts/{id}", async (int id, IForumRepository repo) => await repo.DeletePostAsync(id))
            .WithName("DeletePost").WithSummary("Delete a post by ID");
    }
}
