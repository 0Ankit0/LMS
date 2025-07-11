using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services
{
    public interface IDropdownService
    {
        Task<List<DropdownOption>> GetCategoriesAsync(string? search = null, int take = 20);
        Task<List<DropdownOption>> GetCoursesAsync(string? search = null, int take = 20);
        Task<List<DropdownOption>> GetModulesAsync(string? search = null, int take = 20);
        Task<List<DropdownOption>> GetModulesByCourseAsync(int courseId, string? search = null, int take = 20);
        Task<List<DropdownOption<string>>> GetUsersAsync(string? search = null, int take = 20);
        Task<List<DropdownOption<string>>> GetInstructorsAsync(string? search = null, int take = 20);
        Task<List<DropdownOption>> GetTagsAsync(string? search = null, int take = 20);
        Task<List<DropdownOption>> GetForumsAsync(string? search = null, int take = 20);
        Task<List<DropdownOption>> GetAssessmentsAsync(string? search = null, int take = 20);
        Task<List<DropdownOption>> GetLessonsAsync(string? search = null, int take = 20);
        Task<List<DropdownOption>> GetLessonsByModuleAsync(int moduleId, string? search = null, int take = 20);
    }

    public class DropdownService : IDropdownService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public DropdownService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<DropdownOption>> GetCategoriesAsync(string? search = null, int take = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Categories.Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search) || (c.Description != null && c.Description.Contains(search)));
            }

            return await query
                .OrderBy(c => c.Name)
                .Take(take)
                .Select(c => new DropdownOption
                {
                    Value = c.Id,
                    Text = c.Name,
                    Description = c.Description
                })
                .ToListAsync();
        }

        public async Task<List<DropdownOption>> GetCoursesAsync(string? search = null, int take = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Courses.Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Title.Contains(search) || (c.Description != null && c.Description.Contains(search)));
            }

            return await query
                .OrderBy(c => c.Title)
                .Take(take)
                .Select(c => new DropdownOption
                {
                    Value = c.Id,
                    Text = c.Title,
                    Description = c.Description
                })
                .ToListAsync();
        }

        public async Task<List<DropdownOption>> GetModulesAsync(string? search = null, int take = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            IQueryable<Module> query = context.Modules.Include(m => m.Course);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Title.Contains(search) || (m.Description != null && m.Description.Contains(search)));
            }

            return await query
                .OrderBy(m => m.Title)
                .Take(take)
                .Select(m => new DropdownOption
                {
                    Value = m.Id,
                    Text = $"{m.Title} ({m.Course!.Title})",
                    Description = m.Description
                })
                .ToListAsync();
        }

        public async Task<List<DropdownOption>> GetModulesByCourseAsync(int courseId, string? search = null, int take = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Modules.Where(m => m.CourseId == courseId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Title.Contains(search) || (m.Description != null && m.Description.Contains(search)));
            }

            return await query
                .OrderBy(m => m.OrderIndex)
                .ThenBy(m => m.Title)
                .Take(take)
                .Select(m => new DropdownOption
                {
                    Value = m.Id,
                    Text = m.Title,
                    Description = m.Description
                })
                .ToListAsync();
        }

        public async Task<List<DropdownOption<string>>> GetUsersAsync(string? search = null, int take = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => (u.Email != null && u.Email.Contains(search)) ||
                                   (u.FirstName != null && u.FirstName.Contains(search)) ||
                                   (u.LastName != null && u.LastName.Contains(search)));
            }

            return await query
                .OrderBy(u => u.Email)
                .Take(take)
                .Select(u => new DropdownOption<string>
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName} ({u.Email})",
                    Description = u.Email
                })
                .ToListAsync();
        }

        public async Task<List<DropdownOption<string>>> GetInstructorsAsync(string? search = null, int take = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            // Get users who have created courses (instructors)
            var query = context.Users.Where(u => u.CreatedCourses.Any());

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => (u.Email != null && u.Email.Contains(search)) ||
                                   (u.FirstName != null && u.FirstName.Contains(search)) ||
                                   (u.LastName != null && u.LastName.Contains(search)));
            }

            return await query
                .OrderBy(u => u.Email)
                .Take(take)
                .Select(u => new DropdownOption<string>
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName}",
                    Description = u.Email
                })
                .ToListAsync();
        }

        public async Task<List<DropdownOption>> GetTagsAsync(string? search = null, int take = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Tags.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Name.Contains(search));
            }

            return await query
                .OrderBy(t => t.Name)
                .Take(take)
                .Select(t => new DropdownOption
                {
                    Value = t.Id,
                    Text = t.Name,
                    Description = null
                })
                .ToListAsync();
        }

        public async Task<List<DropdownOption>> GetForumsAsync(string? search = null, int take = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            IQueryable<Forum> query = context.Forums.Include(f => f.Course);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f => f.Title.Contains(search) || (f.Description != null && f.Description.Contains(search)));
            }

            return await query
                .OrderBy(f => f.Title)
                .Take(take)
                .Select(f => new DropdownOption
                {
                    Value = f.Id,
                    Text = $"{f.Title} ({f.Course!.Title})",
                    Description = f.Description
                })
                .ToListAsync();
        }

        public async Task<List<DropdownOption>> GetAssessmentsAsync(string? search = null, int take = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            IQueryable<Assessment> query = context.Assessments.Include(a => a.Course);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.Title.Contains(search) || (a.Description != null && a.Description.Contains(search)));
            }

            return await query
                .OrderBy(a => a.Title)
                .Take(take)
                .Select(a => new DropdownOption
                {
                    Value = a.Id,
                    Text = $"{a.Title} ({a.Course!.Title})",
                    Description = a.Description
                })
                .ToListAsync();
        }

        public async Task<List<DropdownOption>> GetLessonsAsync(string? search = null, int take = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            IQueryable<Lesson> query = context.Lessons.Include(l => l.Module).ThenInclude(m => m.Course);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => l.Title.Contains(search) || (l.Description != null && l.Description.Contains(search)));
            }

            return await query
                .OrderBy(l => l.Title)
                .Take(take)
                .Select(l => new DropdownOption
                {
                    Value = l.Id,
                    Text = $"{l.Title} ({l.Module!.Course!.Title})",
                    Description = l.Description
                })
                .ToListAsync();
        }

        public async Task<List<DropdownOption>> GetLessonsByModuleAsync(int moduleId, string? search = null, int take = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Lessons.Where(l => l.ModuleId == moduleId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => l.Title.Contains(search) || (l.Description != null && l.Description.Contains(search)));
            }

            return await query
                .OrderBy(l => l.OrderIndex)
                .ThenBy(l => l.Title)
                .Take(take)
                .Select(l => new DropdownOption
                {
                    Value = l.Id,
                    Text = l.Title,
                    Description = l.Description
                })
                .ToListAsync();
        }
    }
}
