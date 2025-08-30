using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LMS.Repositories
{
    public interface IMessageRepository
    {
        Task<List<MessageModel>> GetMessagesAsync(string userId);
        Task<List<MessageModel>> GetInboxMessagesAsync(string userId);
        Task<List<MessageModel>> GetSentMessagesAsync(string userId);
        Task<MessageModel?> GetMessageByIdAsync(int id);
        Task<MessageModel> SendMessageAsync(CreateMessageRequest request, string fromUserId);
        Task<bool> MarkMessageAsReadAsync(int messageId, string userId);
        Task<bool> DeleteMessageAsync(int messageId, string userId);
        Task<int> GetUnreadMessageCountAsync(string userId);
    }

    public class MessageRepository : IMessageRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<MessageRepository> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<MessageModel>> GetMessagesAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var messages = await context.Messages
                    .Include(m => m.FromUser)
                    .Include(m => m.ToUser)
                    .Include(m => m.Attachments)
                    .Where(m => (m.FromUserId == userId || m.ToUserId == userId) && !m.IsDeleted)
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();

                return messages.Select(MapToMessageModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching messages for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<MessageModel>> GetInboxMessagesAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var messages = await context.Messages
                    .Include(m => m.FromUser)
                    .Include(m => m.ToUser)
                    .Include(m => m.Attachments)
                    .Where(m => m.ToUserId == userId && !m.IsDeleted)
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();

                return messages.Select(MapToMessageModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inbox messages for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<MessageModel>> GetSentMessagesAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var messages = await context.Messages
                    .Include(m => m.FromUser)
                    .Include(m => m.ToUser)
                    .Include(m => m.Attachments)
                    .Where(m => m.FromUserId == userId && !m.IsDeleted)
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();

                return messages.Select(MapToMessageModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sent messages for user {UserId}", userId);
                throw;
            }
        }

        public async Task<MessageModel?> GetMessageByIdAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var message = await context.Messages
                    .Include(m => m.FromUser)
                    .Include(m => m.ToUser)
                    .Include(m => m.ParentMessage)
                    .Include(m => m.Replies)
                    .Include(m => m.Attachments)
                    .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

                return message != null ? MapToMessageModel(message) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching message by ID {MessageId}", id);
                throw;
            }
        }

        public async Task<MessageModel> SendMessageAsync(CreateMessageRequest request, string fromUserId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var message = new Message
                {
                    Subject = request.Subject,
                    Content = request.Content,
                    FromUserId = fromUserId,
                    ToUserId = request.ToUserId,
                    ParentMessageId = request.ParentMessageId,
                    Priority = (MessagePriority)request.Priority,
                    SentAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                context.Messages.Add(message);
                await context.SaveChangesAsync();
                return await GetMessageByIdAsync(message.Id) ?? throw new InvalidOperationException("Failed to retrieve sent message");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from user {FromUserId}", fromUserId);
                throw;
            }
        }

        public async Task<bool> MarkMessageAsReadAsync(int messageId, string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var message = await context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.ToUserId == userId);

                if (message == null)
                    return false;

                if (message.ReadAt == null)
                {
                    message.ReadAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteMessageAsync(int messageId, string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var message = await context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId && (m.FromUserId == userId || m.ToUserId == userId));

                if (message == null)
                    return false;

                message.IsDeleted = true;
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadMessageCountAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Messages
                    .CountAsync(m => m.ToUserId == userId && m.ReadAt == null && !m.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unread message count for user {UserId}", userId);
                throw;
            }
        }

        private static MessageModel MapToMessageModel(Message message)
        {
            return new MessageModel
            {
                Id = message.Id,
                ConversationId = 0, // Set to 0 for now - implement conversation grouping later if needed
                SenderId = message.FromUserId,
                SenderName = message.FromUser?.UserName ?? "",
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.ReadAt.HasValue,
                ReadAt = message.ReadAt,
                IsEdited = false, // No editing tracking for now
                EditedAt = null,
                Attachments = message.Attachments?.Select(a => new MessageAttachmentModel
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileUrl = a.FilePath, // Map FilePath to FileUrl
                    FileSize = a.FileSize,
                    ContentType = a.ContentType
                }).ToList() ?? new List<MessageAttachmentModel>()
            };
        }
    }
}
