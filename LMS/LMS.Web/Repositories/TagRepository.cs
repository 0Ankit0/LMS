using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using LMS.Data.DTOs;
using LMS.Web.Data;

namespace LMS.Repositories
{
    public interface ITagRepository
    {
        Task<List<TagModel>> GetTagsAsync();
        Task<PaginatedResult<TagModel>> GetTagsPaginatedAsync(PaginationRequest request);
        Task<TagModel?> GetTagByIdAsync(int id);
        Task<TagModel> CreateTagAsync(CreateTagRequest request);
        Task<TagModel> UpdateTagAsync(int id, CreateTagRequest request);
        Task<bool> DeleteTagAsync(int id);
    }

    public class TagRepository : ITagRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<TagRepository> _logger;

        public TagRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<TagRepository> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<TagModel>> GetTagsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var tags = await context.Tags
                    .OrderBy(t => t.Name)
                    .ToListAsync();
                return tags.Select(MapToTagModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags");
                throw;
            }
        }

        public async Task<PaginatedResult<TagModel>> GetTagsPaginatedAsync(PaginationRequest request)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Tags
                    .OrderBy(t => t.Name);

                var totalCount = await query.CountAsync();

                var tags = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return new PaginatedResult<TagModel>
                {
                    Items = tags.Select(MapToTagModel).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated tags");
                throw;
            }
        }

        public async Task<TagModel?> GetTagByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var tag = await context.Tags.FindAsync(id);
                return tag != null ? MapToTagModel(tag) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag by id: {TagId}", id);
                throw;
            }
        }

        public async Task<TagModel> CreateTagAsync(CreateTagRequest request)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var tag = new Tag
                {
                    Name = request.Name,
                    Color = request.Color
                };
                context.Tags.Add(tag);
                await context.SaveChangesAsync();
                return MapToTagModel(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                throw;
            }
        }

        public async Task<TagModel> UpdateTagAsync(int id, CreateTagRequest request)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var tag = await context.Tags.FindAsync(id);
                if (tag == null)
                    throw new ArgumentException($"Tag with ID {id} not found");

                tag.Name = request.Name;
                tag.Color = request.Color;

                await context.SaveChangesAsync();

                return MapToTagModel(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag: {TagId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var tag = await context.Tags.FindAsync(id);
                if (tag == null)
                    return false;

                context.Tags.Remove(tag);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag: {TagId}", id);
                throw;
            }
        }

        private static TagModel MapToTagModel(Tag tag)
        {
            return new TagModel
            {
                Id = tag.Id,
                Name = tag.Name,
                Color = tag.Color
            };
        }
    }
}
