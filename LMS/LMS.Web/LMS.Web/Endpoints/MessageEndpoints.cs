
using LMS.Repositories;
using LMS.Web.Infrastructure;
using LMS.Data.DTOs;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Endpoints;

public class MessageEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/messages");
        group.MapGet("/user/{userId}", async (string userId, IMessageRepository repo) => await repo.GetMessagesAsync(userId));
        group.MapGet("/user/{userId}/inbox", async (string userId, IMessageRepository repo) => await repo.GetInboxMessagesAsync(userId));
        group.MapGet("/user/{userId}/sent", async (string userId, IMessageRepository repo) => await repo.GetSentMessagesAsync(userId));
        group.MapGet("/{id}", async (int id, IMessageRepository repo) => await repo.GetMessageByIdAsync(id));
        group.MapPost("/send", async (CreateMessageRequest req, string fromUserId, IMessageRepository repo) => await repo.SendMessageAsync(req, fromUserId));
        group.MapPut("/{messageId}/read", async (int messageId, string userId, IMessageRepository repo) => await repo.MarkMessageAsReadAsync(messageId, userId));
        group.MapDelete("/{messageId}", async (int messageId, string userId, IMessageRepository repo) => await repo.DeleteMessageAsync(messageId, userId));
        group.MapGet("/user/{userId}/unread-count", async (string userId, IMessageRepository repo) => await repo.GetUnreadMessageCountAsync(userId));
    }
}
