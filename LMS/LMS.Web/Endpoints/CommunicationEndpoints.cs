using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Infrastructure;
using LMS.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LMS.Web.Endpoints
{
    public class CommunicationEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var conversationsGroup = app.MapGroup("/api/conversations").WithTags("Communication");
            var messagesGroup = app.MapGroup("/api/messages").WithTags("Communication");
            var forumGroup = app.MapGroup("/api/forum-topics").WithTags("Communication");

            // Conversations endpoints
            conversationsGroup.MapPost("", CreateConversation)
                .RequireAuthorization()
                .WithName("CreateConversation")
                .WithSummary("Create a new private or group conversation");

            conversationsGroup.MapGet("", GetUserConversations)
                .RequireAuthorization()
                .WithName("GetUserConversations")
                .WithSummary("Get all conversations for the current user");

            conversationsGroup.MapGet("/{id:int}/messages", GetConversationMessages)
                .RequireAuthorization()
                .WithName("GetConversationMessages")
                .WithSummary("Get all messages for a conversation");

            // Messages endpoints
            messagesGroup.MapPost("", SendMessage)
                .RequireAuthorization()
                .WithName("SendMessage")
                .WithSummary("Send a new message to a conversation");

            messagesGroup.MapPut("/{id:int}/read", MarkMessageAsRead)
                .RequireAuthorization()
                .WithName("MarkMessageAsRead")
                .WithSummary("Mark a message as read");

            // Forum endpoints
            forumGroup.MapPost("/{id:int}/posts", AddForumPost)
                .RequireAuthorization()
                .WithName("AddForumPost")
                .WithSummary("Add a new post to a forum topic");

            forumGroup.MapGet("/{id:int}/posts", GetForumPosts)
                .RequireAuthorization()
                .WithName("GetForumPosts")
                .WithSummary("Get all posts for a forum topic");

            forumGroup.MapGet("", GetForumTopics)
                .RequireAuthorization()
                .WithName("GetForumTopics")
                .WithSummary("Get all forum topics");
        }

        private static async Task<IResult> CreateConversation(
            CreateConversationRequest request,
            IForumRepository forumRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                // For now, assume single conversation with first participant
                var targetUserId = request.ParticipantIds.FirstOrDefault();
                if (string.IsNullOrEmpty(targetUserId))
                {
                    return Results.BadRequest("Target user ID is required");
                }

                var conversation = await forumRepository.CreateConversationAsync(userId, targetUserId);
                return Results.Ok(conversation);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating conversation: {ex.Message}");
            }
        }

        private static async Task<IResult> GetUserConversations(
            IForumRepository forumRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var conversations = await forumRepository.GetUserConversationsAsync(userId);
                return Results.Ok(conversations);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving conversations: {ex.Message}");
            }
        }

        private static async Task<IResult> GetConversationMessages(
            int id,
            IForumRepository forumRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                // Check if user has access to this conversation
                var hasAccess = await forumRepository.HasConversationAccessAsync(userId, id);
                if (!hasAccess)
                {
                    return Results.Forbid();
                }

                var messages = await forumRepository.GetConversationMessagesAsync(id);
                return Results.Ok(messages);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving conversation messages: {ex.Message}");
            }
        }

        private static async Task<IResult> SendMessage(
            SendMessageRequest request,
            IForumRepository forumRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                // Check if user has access to this conversation
                var hasAccess = await forumRepository.HasConversationAccessAsync(userId, request.ConversationId);
                if (!hasAccess)
                {
                    return Results.Forbid();
                }

                var message = await forumRepository.SendMessageAsync(request.ConversationId, userId, request.Content);
                return Results.Ok(message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error sending message: {ex.Message}");
            }
        }

        private static async Task<IResult> MarkMessageAsRead(
            int id,
            IForumRepository forumRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var success = await forumRepository.MarkMessageAsReadAsync(id, userId);
                if (!success)
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error marking message as read: {ex.Message}");
            }
        }

        private static async Task<IResult> AddForumPost(
            int id,
            CreateForumPostRequest request,
            IForumRepository forumRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                // Set the topic ID from the route parameter
                request.TopicId = id;

                var post = await forumRepository.CreateForumPostAsync(request, userId);
                return Results.Ok(post);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error adding forum post: {ex.Message}");
            }
        }

        private static async Task<IResult> GetForumPosts(
            int id,
            IForumRepository forumRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var posts = await forumRepository.GetForumPostsAsync(id);
                return Results.Ok(posts);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving forum posts: {ex.Message}");
            }
        }

        private static async Task<IResult> GetForumTopics(
            IForumRepository forumRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var topics = await forumRepository.GetAllForumTopicsAsync();
                return Results.Ok(topics);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving forum topics: {ex.Message}");
            }
        }
    }
}
