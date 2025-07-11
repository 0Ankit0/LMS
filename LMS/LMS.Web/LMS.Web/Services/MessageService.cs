using LMS.Data.Entities;
using LMS.Models.Communication;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services
{
    public interface IMessageService
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

    public class MessageService : IMessageService
    {
        private readonly AuthDbContext _context;

        public MessageService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<List<MessageModel>> GetMessagesAsync(string userId)
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

        public async Task<List<MessageModel>> GetInboxMessagesAsync(string userId)
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

        public async Task<List<MessageModel>> GetSentMessagesAsync(string userId)
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

        public async Task<MessageModel?> GetMessageByIdAsync(int id)
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

        public async Task<MessageModel> SendMessageAsync(CreateMessageRequest request, string fromUserId)
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

        public async Task<bool> MarkMessageAsReadAsync(int messageId, string userId)
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

        public async Task<bool> DeleteMessageAsync(int messageId, string userId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && (m.FromUserId == userId || m.ToUserId == userId));

            if (message == null)
                return false;

            message.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadMessageCountAsync(string userId)
        {
            return await _context.Messages
                .CountAsync(m => m.ToUserId == userId && m.ReadAt == null && !m.IsDeleted);
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
