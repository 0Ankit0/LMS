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
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TagRepository> _logger;

        public TagRepository(ApplicationDbContext context, ILogger<TagRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<TagModel>> GetTagsAsync()
        {
            try
            {
                var tags = await _context.Tags
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
                var query = _context.Tags
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
                var tag = await _context.Tags.FindAsync(id);
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
                var tag = new Tag
                {
                    Name = request.Name,
                    Color = request.Color
                };

                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();

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
                var tag = await _context.Tags.FindAsync(id);
                if (tag == null)
                    throw new ArgumentException($"Tag with ID {id} not found");

                tag.Name = request.Name;
                tag.Color = request.Color;

                await _context.SaveChangesAsync();

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
                var tag = await _context.Tags.FindAsync(id);
                if (tag == null)
                    return false;

                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
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
