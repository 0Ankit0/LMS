using LMS.Data;
using LMS.Models.Course;
using LMS.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services
{
    public interface ITagService
    {
        Task<List<TagModel>> GetTagsAsync();
        Task<PaginatedResult<TagModel>> GetTagsPaginatedAsync(PaginationRequest request);
        Task<TagModel?> GetTagByIdAsync(int id);
        Task<TagModel> CreateTagAsync(CreateTagRequest request);
        Task<TagModel> UpdateTagAsync(int id, CreateTagRequest request);
        Task<bool> DeleteTagAsync(int id);
    }

    public class TagService : ITagService
    {
        private readonly IDbContextFactory<AuthDbContext> _contextFactory;

        public TagService(IDbContextFactory<AuthDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<TagModel>> GetTagsAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            var tags = await context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();

            return tags.Select(MapToTagModel).ToList();
        }

        public async Task<PaginatedResult<TagModel>> GetTagsPaginatedAsync(PaginationRequest request)
        {
            await using var context = _contextFactory.CreateDbContext();

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

        public async Task<TagModel?> GetTagByIdAsync(int id)
        {
            await using var context = _contextFactory.CreateDbContext();
            var tag = await context.Tags.FindAsync(id);
            return tag != null ? MapToTagModel(tag) : null;
        }

        public async Task<TagModel> CreateTagAsync(CreateTagRequest request)
        {
            await using var context = _contextFactory.CreateDbContext();

            var tag = new Tag
            {
                Name = request.Name,
                Color = request.Color
            };

            context.Tags.Add(tag);
            await context.SaveChangesAsync();

            return MapToTagModel(tag);
        }

        public async Task<TagModel> UpdateTagAsync(int id, CreateTagRequest request)
        {
            await using var context = _contextFactory.CreateDbContext();

            var tag = await context.Tags.FindAsync(id);
            if (tag == null)
                throw new ArgumentException($"Tag with ID {id} not found");

            tag.Name = request.Name;
            tag.Color = request.Color;

            await context.SaveChangesAsync();

            return MapToTagModel(tag);
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            await using var context = _contextFactory.CreateDbContext();

            var tag = await context.Tags.FindAsync(id);
            if (tag == null)
                return false;

            context.Tags.Remove(tag);
            await context.SaveChangesAsync();
            return true;
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
