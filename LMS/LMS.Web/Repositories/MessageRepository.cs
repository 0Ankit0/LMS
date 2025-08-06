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
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(ApplicationDbContext context, ILogger<MessageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<MessageModel>> GetMessagesAsync(string userId)
        {
            try
            {
                var messages = await _context.Messages
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
                var messages = await _context.Messages
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
                var messages = await _context.Messages
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
                var message = await _context.Messages
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

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

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
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.ToUserId == userId);

                if (message == null)
                    return false;

                if (message.ReadAt == null)
                {
                    message.ReadAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
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
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId && (m.FromUserId == userId || m.ToUserId == userId));

                if (message == null)
                    return false;

                message.IsDeleted = true;
                await _context.SaveChangesAsync();
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
                return await _context.Messages
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
                Subject = message.Subject,
                Content = message.Content,
                FromUserId = message.FromUserId,
                FromUserName = message.FromUser?.UserName ?? "",
                ToUserId = message.ToUserId,
                ToUserName = message.ToUser?.UserName ?? "",
                ParentMessageId = message.ParentMessageId,
                SentAt = message.SentAt,
                ReadAt = message.ReadAt,
                Priority = message.Priority.ToString(),
                Attachments = message.Attachments?.Select(a => new MessageAttachmentModel
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    FileSize = a.FileSize,
                    ContentType = a.ContentType,
                    UploadedAt = a.UploadedAt
                }).ToList() ?? new List<MessageAttachmentModel>()
            };
        }
    }
}
