using LMS.Data;
using LMS.Models.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LMS.Services
{
    public interface IForumService
    {
        Task<List<ForumModel>> GetForumsAsync();
        Task<ForumModel?> GetForumByIdAsync(int id);
        Task<List<ForumModel>> GetForumsByCourseIdAsync(int courseId);
        Task<ForumModel> CreateForumAsync(CreateForumRequest request);
        Task<ForumModel> UpdateForumAsync(int id, CreateForumRequest request);
        Task<bool> DeleteForumAsync(int id);

        // Forum Topics
        Task<List<ForumTopicModel>> GetTopicsByForumIdAsync(int forumId);
        Task<ForumTopicModel?> GetTopicByIdAsync(int id);
        Task<ForumTopicModel> CreateTopicAsync(CreateForumTopicRequest request, string userId);
        Task<bool> DeleteTopicAsync(int id);

        // Forum Posts
        Task<List<ForumPostModel>> GetPostsByTopicIdAsync(int topicId);
        Task<ForumPostModel?> GetPostByIdAsync(int id);
        Task<ForumPostModel> CreatePostAsync(CreateForumPostRequest request, string authorId);
        Task<bool> DeletePostAsync(int id);
    }

    public class ForumService : IForumService
    {
        private readonly IDbContextFactory<AuthDbContext> _contextFactory;

        public ForumService(IDbContextFactory<AuthDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ForumModel>> GetForumsAsync()
        {
            await using var _context = _contextFactory.CreateDbContext();
            var forums = await _context.Forums
                .Include(f => f.Course)
                .Include(f => f.Topics)
                .OrderBy(f => f.Title)
                .ToListAsync();

            return forums.Select(MapToForumModel).ToList();
        }

        public async Task<ForumModel?> GetForumByIdAsync(int id)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var forum = await _context.Forums
                .Include(f => f.Course)
                .Include(f => f.Topics)
                .ThenInclude(t => t.Posts)
                .FirstOrDefaultAsync(f => f.Id == id);

            return forum != null ? MapToForumModel(forum) : null;
        }

        public async Task<List<ForumModel>> GetForumsByCourseIdAsync(int courseId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var forums = await _context.Forums
                .Include(f => f.Course)
                .Include(f => f.Topics)
                .Where(f => f.CourseId == courseId)
                .OrderBy(f => f.Title)
                .ToListAsync();

            return forums.Select(MapToForumModel).ToList();
        }

        public async Task<ForumModel> CreateForumAsync(CreateForumRequest request)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var forum = new Forum
            {
                Title = request.Title,
                Description = request.Description,
                CourseId = request.CourseId,
                IsGeneral = request.CourseId == null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Forums.Add(forum);
            await _context.SaveChangesAsync();

            return await GetForumByIdAsync(forum.Id) ?? throw new InvalidOperationException("Failed to retrieve created forum");
        }

        public async Task<ForumModel> UpdateForumAsync(int id, CreateForumRequest request)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var forum = await _context.Forums.FindAsync(id);
            if (forum == null)
                throw new ArgumentException("Forum not found", nameof(id));

            forum.Title = request.Title;
            forum.Description = request.Description;

            await _context.SaveChangesAsync();

            return await GetForumByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated forum");
        }

        public async Task<bool> DeleteForumAsync(int id)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var forum = await _context.Forums.FindAsync(id);
            if (forum == null)
                return false;

            _context.Forums.Remove(forum);
            await _context.SaveChangesAsync();
            return true;
        }

        // Forum Topics
        public async Task<List<ForumTopicModel>> GetTopicsByForumIdAsync(int forumId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var topics = await _context.ForumTopics
                .Include(t => t.Forum)
                .Include(t => t.CreatedBy)
                .Include(t => t.Posts)
                .Where(t => t.ForumId == forumId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return topics.Select(MapToForumTopicModel).ToList();
        }

        public async Task<ForumTopicModel?> GetTopicByIdAsync(int id)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var topic = await _context.ForumTopics
                .Include(t => t.Forum)
                .Include(t => t.CreatedBy)
                .Include(t => t.Posts)
                .ThenInclude(p => p.Author)
                .FirstOrDefaultAsync(t => t.Id == id);

            return topic != null ? MapToForumTopicModel(topic) : null;
        }

        public async Task<ForumTopicModel> CreateTopicAsync(CreateForumTopicRequest request, string userId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var topic = new ForumTopic
            {
                Title = request.Title,
                ForumId = request.ForumId,
                CreatedByUserId = userId,
                IsLocked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumTopics.Add(topic);
            await _context.SaveChangesAsync();

            // Create initial post for the topic
            var initialPost = new ForumPost
            {
                Content = request.InitialPost,
                TopicId = topic.Id,
                AuthorId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumPosts.Add(initialPost);
            topic.LastPostAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetTopicByIdAsync(topic.Id) ?? throw new InvalidOperationException("Failed to retrieve created topic");
        }

        public async Task<bool> DeleteTopicAsync(int id)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var topic = await _context.ForumTopics.FindAsync(id);
            if (topic == null)
                return false;

            _context.ForumTopics.Remove(topic);
            await _context.SaveChangesAsync();
            return true;
        }

        // Forum Posts
        public async Task<List<ForumPostModel>> GetPostsByTopicIdAsync(int topicId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var posts = await _context.ForumPosts
                .Include(p => p.Topic)
                .Include(p => p.Author)
                .Where(p => p.TopicId == topicId)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            return posts.Select(MapToForumPostModel).ToList();
        }

        public async Task<ForumPostModel?> GetPostByIdAsync(int id)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var post = await _context.ForumPosts
                .Include(p => p.Topic)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            return post != null ? MapToForumPostModel(post) : null;
        }

        public async Task<ForumPostModel> CreatePostAsync(CreateForumPostRequest request, string authorId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var post = new ForumPost
            {
                Content = request.Content,
                TopicId = request.TopicId,
                AuthorId = authorId,
                ParentPostId = request.ParentPostId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumPosts.Add(post);

            // Update topic's last post info
            var topic = await _context.ForumTopics.FindAsync(request.TopicId);
            if (topic != null)
            {
                topic.LastPostAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return await GetPostByIdAsync(post.Id) ?? throw new InvalidOperationException("Failed to retrieve created post");
        }

        public async Task<bool> DeletePostAsync(int id)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
                return false;

            _context.ForumPosts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ForumModel MapToForumModel(Forum forum)
        {
            return new ForumModel
            {
                Id = forum.Id,
                Title = forum.Title,
                Description = forum.Description,
                CourseId = forum.CourseId,
                CourseName = forum.Course?.Title ?? "",
                TopicCount = forum.Topics?.Count ?? 0,
                IsGeneral = forum.IsGeneral,
                IsActive = forum.IsActive,
                CreatedAt = forum.CreatedAt,
                LastPostAt = forum.Topics?.Max(t => t.LastPostAt),
                Topics = new List<ForumTopicModel>()
            };
        }

        private static ForumTopicModel MapToForumTopicModel(ForumTopic topic)
        {
            return new ForumTopicModel
            {
                Id = topic.Id,
                Title = topic.Title,
                ForumId = topic.ForumId,
                ForumTitle = topic.Forum?.Title ?? "",
                CreatedByUserId = topic.CreatedByUserId,
                CreatedByUserName = topic.CreatedBy?.UserName ?? "",
                IsLocked = topic.IsLocked,
                PostCount = topic.Posts?.Count ?? 0,
                CreatedAt = topic.CreatedAt,
                LastPostAt = topic.LastPostAt,
                Posts = new List<ForumPostModel>()
            };
        }

        private static ForumPostModel MapToForumPostModel(ForumPost post)
        {
            return new ForumPostModel
            {
                Id = post.Id,
                Content = post.Content,
                TopicId = post.TopicId,
                TopicTitle = post.Topic?.Title ?? "",
                AuthorId = post.AuthorId,
                AuthorName = post.Author?.UserName ?? "",
                ParentPostId = post.ParentPostId,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                IsDeleted = post.IsDeleted,
                Replies = new List<ForumPostModel>()
            };
        }
    }
}
