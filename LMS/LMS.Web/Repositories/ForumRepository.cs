using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using LMS.Data.DTOs;
using LMS.Web.Data;
using LMS.Web.Components.Pages.User;

namespace LMS.Repositories
{
    public class CourseGroup
    {
        public string CourseName { get; set; } = string.Empty;
        public int ForumCount { get; set; }
    }

    public interface IForumRepository
    {
        Task<List<ForumModel>> GetForumsAsync();
        Task<PaginatedResult<ForumModel>> GetForumsPaginatedAsync(PaginationRequest request);
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
        Task<List<ForumPostModel>> GetAllForumPostsAsync();
        Task<PaginatedResult<ForumPostModel>> GetAllForumPostsPaginatedAsync(PaginationRequest request);
        Task<ForumPostModel?> GetPostByIdAsync(int id);
        Task<ForumPostModel> CreatePostAsync(CreateForumPostRequest request, string authorId);
        Task<ForumPostModel?> UpdatePostAsync(int id, CreateForumPostRequest request, string authorId);
        Task<bool> DeletePostAsync(int id);

        // --- Add missing methods for Forums.razor ---
        Task<List<ForumModel>> GetAllForumsAsync();
        Task<List<CourseGroup>> GetCourseGroupsAsync();
    }

    public class ForumRepository : IForumRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ForumRepository> _logger;

        public ForumRepository(ApplicationDbContext context, ILogger<ForumRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<List<ForumModel>> GetForumsAsync()
        {
            try
            {
                var forums = await _context.Forums
                    .Include(f => f.Course)
                    .Include(f => f.Topics)
                    .OrderBy(f => f.Title)
                    .ToListAsync();
                return forums.Select(MapToForumModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting forums");
                throw;
            }
        }




        public async Task<PaginatedResult<ForumModel>> GetForumsPaginatedAsync(PaginationRequest request)
        {
            try
            {
                request.Validate();
                var query = _context.Forums.AsQueryable()
                    .Include(f => f.Course)
                    .Include(f => f.Topics)
                    .OrderBy(f => f.Title);

                var totalCount = await query.CountAsync();
                var skip = (request.PageNumber - 1) * request.PageSize;
                var forums = await query.Skip(skip).Take(request.PageSize).ToListAsync();

                var forumModels = forums.Select(MapToForumModel).ToList();

                return new PaginatedResult<ForumModel>
                {
                    Items = forumModels,
                    TotalCount = totalCount,
                    PageSize = request.PageSize,
                    PageNumber = request.PageNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated forums");
                throw;
            }
        }


        public async Task<ForumModel?> GetForumByIdAsync(int id)
        {
            try
            {
                var forum = await _context.Forums
                    .Include(f => f.Course)
                    .Include(f => f.Topics)
                    .ThenInclude(t => t.Posts)
                    .FirstOrDefaultAsync(f => f.Id == id);
                return forum != null ? MapToForumModel(forum) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting forum by id: {Id}", id);
                throw;
            }
        }


        public async Task<List<ForumModel>> GetForumsByCourseIdAsync(int courseId)
        {
            try
            {
                var forums = await _context.Forums
                    .Include(f => f.Course)
                    .Include(f => f.Topics)
                    .Where(f => f.CourseId == courseId)
                    .OrderBy(f => f.Title)
                    .ToListAsync();
                return forums.Select(MapToForumModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting forums by course id: {CourseId}", courseId);
                throw;
            }
        }



        public async Task<ForumModel> CreateForumAsync(CreateForumRequest request)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating forum");
                throw;
            }
        }




        public async Task<ForumModel> UpdateForumAsync(int id, CreateForumRequest request)
        {
            try
            {
                var forum = await _context.Forums.FindAsync(id);
                if (forum == null)
                    throw new ArgumentException("Forum not found", nameof(id));

                forum.Title = request.Title;
                forum.Description = request.Description;

                await _context.SaveChangesAsync();

                return await GetForumByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated forum");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating forum: {Id}", id);
                throw;
            }
        }


        public async Task<bool> DeleteForumAsync(int id)
        {
            try
            {
                var forum = await _context.Forums.FindAsync(id);
                if (forum == null)
                    return false;

                _context.Forums.Remove(forum);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting forum: {Id}", id);
                throw;
            }
        }

        // Forum Topics

        public async Task<List<ForumTopicModel>> GetTopicsByForumIdAsync(int forumId)
        {
            try
            {
                var topics = await _context.ForumTopics
                    .Include(t => t.Forum)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.Posts)
                    .Where(t => t.ForumId == forumId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
                return topics.Select(MapToForumTopicModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting topics by forum id: {ForumId}", forumId);
                throw;
            }
        }


        public async Task<ForumTopicModel?> GetTopicByIdAsync(int id)
        {
            try
            {
                var topic = await _context.ForumTopics
                    .Include(t => t.Forum)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.Posts)
                    .ThenInclude(p => p.Author)
                    .FirstOrDefaultAsync(t => t.Id == id);
                return topic != null ? MapToForumTopicModel(topic) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting topic by id: {Id}", id);
                throw;
            }
        }





        public async Task<ForumTopicModel> CreateTopicAsync(CreateForumTopicRequest request, string userId)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating topic for forum: {ForumId}", request.ForumId);
                throw;
            }
        }


        public async Task<bool> DeleteTopicAsync(int id)
        {
            try
            {
                var topic = await _context.ForumTopics.FindAsync(id);
                if (topic == null)
                    return false;

                _context.ForumTopics.Remove(topic);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting topic: {Id}", id);
                throw;
            }
        }

        // Forum Posts

        public async Task<List<ForumPostModel>> GetPostsByTopicIdAsync(int topicId)
        {
            try
            {
                var posts = await _context.ForumPosts
                    .Include(p => p.Topic)
                    .Include(p => p.Author)
                    .Where(p => p.TopicId == topicId)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();
                return posts.Select(MapToForumPostModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts by topic id: {TopicId}", topicId);
                throw;
            }
        }


        public async Task<List<ForumPostModel>> GetAllForumPostsAsync()
        {
            try
            {
                var posts = await _context.ForumPosts
                    .Include(p => p.Topic)
                        .ThenInclude(t => t.Forum)
                    .Include(p => p.Author)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
                return posts.Select(MapToForumPostModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all forum posts");
                throw;
            }
        }




        public async Task<PaginatedResult<ForumPostModel>> GetAllForumPostsPaginatedAsync(PaginationRequest request)
        {
            try
            {
                request.Validate();
                var query = _context.ForumPosts.AsQueryable()
                    .Include(p => p.Topic)
                        .ThenInclude(t => t.Forum)
                    .Include(p => p.Author)
                    .OrderByDescending(p => p.CreatedAt);

                var totalCount = await query.CountAsync();
                var skip = (request.PageNumber - 1) * request.PageSize;
                var posts = await query.Skip(skip).Take(request.PageSize).ToListAsync();

                var postModels = posts.Select(MapToForumPostModel).ToList();

                return new PaginatedResult<ForumPostModel>
                {
                    Items = postModels,
                    TotalCount = totalCount,
                    PageSize = request.PageSize,
                    PageNumber = request.PageNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated forum posts");
                throw;
            }
        }


        public async Task<ForumPostModel?> GetPostByIdAsync(int id)
        {
            try
            {
                var post = await _context.ForumPosts
                    .Include(p => p.Topic)
                    .Include(p => p.Author)
                    .FirstOrDefaultAsync(p => p.Id == id);
                return post != null ? MapToForumPostModel(post) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting forum post by id: {Id}", id);
                throw;
            }
        }





        public async Task<ForumPostModel> CreatePostAsync(CreateForumPostRequest request, string authorId)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating forum post for topic: {TopicId}", request.TopicId);
                throw;
            }
        }

        public async Task<ForumPostModel?> UpdatePostAsync(int id, CreateForumPostRequest request, string authorId)
        {
            try
            {
                var post = await _context.ForumPosts.FindAsync(id);
                if (post == null)
                    return null;
                post.Content = request.Content;
                post.TopicId = request.TopicId;
                post.ParentPostId = request.ParentPostId;
                post.UpdatedAt = DateTime.UtcNow;
                // Optionally update author if needed: post.AuthorId = authorId;
                await _context.SaveChangesAsync();
                return await GetPostByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating forum post: {Id}", id);
                throw;
            }
        }


        public async Task<bool> DeletePostAsync(int id)
        {
            try
            {
                var post = await _context.ForumPosts.FindAsync(id);
                if (post == null)
                    return false;

                _context.ForumPosts.Remove(post);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting forum post: {Id}", id);
                throw;
            }
        }

        public async Task<List<ForumModel>> GetAllForumsAsync()
        {
            // For now, just call GetForumsAsync
            return await GetForumsAsync();
        }

        public async Task<List<CourseGroup>> GetCourseGroupsAsync()
        {
            // Dummy implementation: group forums by CourseName
            var forums = await GetForumsAsync();
            var groups = forums
                .Where(f => !string.IsNullOrEmpty(f.CourseName))
                .GroupBy(f => f.CourseName)
                .Select(g => new CourseGroup
                {
                    CourseName = g.Key ?? "",
                    ForumCount = g.Count()
                })
                .ToList();
            return groups;
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
