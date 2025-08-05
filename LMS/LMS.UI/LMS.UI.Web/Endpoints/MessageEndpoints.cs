
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
        group.MapGet("/user/{userId}", async (string userId, IMessageRepository repo) => await repo.GetMessagesAsync(userId))
            .WithName("GetMessages").WithSummary("Get all messages for a user");
        group.MapGet("/user/{userId}/inbox", async (string userId, IMessageRepository repo) => await repo.GetInboxMessagesAsync(userId))
            .WithName("GetInboxMessages").WithSummary("Get inbox messages for a user");
        group.MapGet("/user/{userId}/sent", async (string userId, IMessageRepository repo) => await repo.GetSentMessagesAsync(userId))
            .WithName("GetSentMessages").WithSummary("Get sent messages for a user");
        group.MapGet("/{id}", async (int id, IMessageRepository repo) => await repo.GetMessageByIdAsync(id))
            .WithName("GetMessageById").WithSummary("Get a message by ID");
        group.MapPost("/send", async (CreateMessageRequest req, string fromUserId, IMessageRepository repo) => await repo.SendMessageAsync(req, fromUserId))
            .WithName("SendMessage").WithSummary("Send a new message");
        group.MapPut("/{messageId}/read", async (int messageId, string userId, IMessageRepository repo) => await repo.MarkMessageAsReadAsync(messageId, userId))
            .WithName("MarkMessageAsRead").WithSummary("Mark a message as read");
        group.MapDelete("/{messageId}", async (int messageId, string userId, IMessageRepository repo) => await repo.DeleteMessageAsync(messageId, userId))
            .WithName("DeleteMessage").WithSummary("Delete a message by ID");
        group.MapGet("/user/{userId}/unread-count", async (string userId, IMessageRepository repo) => await repo.GetUnreadMessageCountAsync(userId))
            .WithName("GetUnreadMessageCount").WithSummary("Get unread message count for a user");
    }
}
