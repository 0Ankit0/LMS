using LMS.Data.DTOs.LMS.Note;
using LMS.Data.Entities;
using LMS.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Web.Repositories
{
    public interface INoteRepository
    {
        Task<IEnumerable<NoteDTO>> GetUserNotesAsync(string userId, NoteFilterDTO? filter = null);
        Task<NoteDTO?> GetNoteByIdAsync(int id, string userId);
        Task<NoteDTO> CreateNoteAsync(CreateNoteDTO createNote, string userId);
        Task<NoteDTO?> UpdateNoteAsync(UpdateNoteDTO updateNote, string userId);
        Task<bool> DeleteNoteAsync(int id, string userId);
        Task<bool> TogglePinNoteAsync(int id, string userId);
        Task<NotesSummaryDTO> GetNotesSummaryAsync(string userId);
        Task<IEnumerable<NoteQuickAccessDTO>> GetPinnedNotesAsync(string userId);
        Task<IEnumerable<NoteQuickAccessDTO>> GetRecentNotesAsync(string userId, int count = 5);
        Task<IEnumerable<string>> GetUserTagsAsync(string userId);
        Task<IEnumerable<NoteDTO>> SearchNotesAsync(string userId, string searchTerm);
        Task<IEnumerable<NoteDTO>> GetNotesForCourseAsync(string userId, int courseId);
        Task<IEnumerable<NoteDTO>> GetNotesForLessonAsync(string userId, int lessonId);
    }

    public class NoteRepository : INoteRepository
    {
        private readonly ApplicationDbContext _context;

        public NoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NoteDTO>> GetUserNotesAsync(string userId, NoteFilterDTO? filter = null)
        {
            var query = _context.Notes
                .Include(n => n.Course)
                .Include(n => n.Lesson)
                .Include(n => n.User)
                .Where(n => n.UserId == userId && !n.IsDeleted);

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    query = query.Where(n => n.Title.ToLower().Contains(searchTerm) ||
                                           n.Content.ToLower().Contains(searchTerm) ||
                                           (n.Tags != null && n.Tags.ToLower().Contains(searchTerm)));
                }

                if (filter.CourseId.HasValue)
                    query = query.Where(n => n.CourseId == filter.CourseId);

                if (filter.LessonId.HasValue)
                    query = query.Where(n => n.LessonId == filter.LessonId);

                if (filter.Type.HasValue)
                    query = query.Where(n => n.Type == filter.Type);

                if (filter.Priority.HasValue)
                    query = query.Where(n => n.Priority == filter.Priority);

                if (filter.IsPrivate.HasValue)
                    query = query.Where(n => n.IsPrivate == filter.IsPrivate);

                if (filter.IsPinned.HasValue)
                    query = query.Where(n => n.IsPinned == filter.IsPinned);

                if (filter.FromDate.HasValue)
                    query = query.Where(n => n.CreatedAt >= filter.FromDate);

                if (filter.ToDate.HasValue)
                    query = query.Where(n => n.CreatedAt <= filter.ToDate);

                if (!string.IsNullOrEmpty(filter.Tags))
                {
                    var tags = filter.Tags.Split(',').Select(t => t.Trim().ToLower());
                    query = query.Where(n => n.Tags != null && tags.Any(tag => n.Tags.ToLower().Contains(tag)));
                }

                // Sorting
                query = filter.SortBy.ToLower() switch
                {
                    "title" => filter.SortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(n => n.Title)
                        : query.OrderBy(n => n.Title),
                    "updatedat" => filter.SortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(n => n.UpdatedAt)
                        : query.OrderBy(n => n.UpdatedAt),
                    "priority" => filter.SortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(n => n.Priority)
                        : query.OrderBy(n => n.Priority),
                    "type" => filter.SortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(n => n.Type)
                        : query.OrderBy(n => n.Type),
                    _ => filter.SortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(n => n.CreatedAt)
                        : query.OrderBy(n => n.CreatedAt)
                };

                // Pagination
                query = query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);
            }
            else
            {
                query = query.OrderByDescending(n => n.CreatedAt);
            }

            var notes = await query.ToListAsync();
            return notes.Select(MapToDTO);
        }

        public async Task<NoteDTO?> GetNoteByIdAsync(int id, string userId)
        {
            var note = await _context.Notes
                .Include(n => n.Course)
                .Include(n => n.Lesson)
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && !n.IsDeleted);

            return note != null ? MapToDTO(note) : null;
        }

        public async Task<NoteDTO> CreateNoteAsync(CreateNoteDTO createNote, string userId)
        {
            var note = new Note
            {
                Title = createNote.Title,
                Content = createNote.Content,
                UserId = userId,
                CourseId = createNote.CourseId,
                LessonId = createNote.LessonId,
                IsPrivate = createNote.IsPrivate,
                IsPinned = createNote.IsPinned,
                Tags = createNote.Tags,
                Type = createNote.Type,
                Priority = createNote.Priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // Reload with includes
            var savedNote = await _context.Notes
                .Include(n => n.Course)
                .Include(n => n.Lesson)
                .Include(n => n.User)
                .FirstAsync(n => n.Id == note.Id);

            return MapToDTO(savedNote);
        }

        public async Task<NoteDTO?> UpdateNoteAsync(UpdateNoteDTO updateNote, string userId)
        {
            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == updateNote.Id && n.UserId == userId && !n.IsDeleted);

            if (note == null) return null;

            note.Title = updateNote.Title;
            note.Content = updateNote.Content;
            note.CourseId = updateNote.CourseId;
            note.LessonId = updateNote.LessonId;
            note.IsPrivate = updateNote.IsPrivate;
            note.IsPinned = updateNote.IsPinned;
            note.Tags = updateNote.Tags;
            note.Type = updateNote.Type;
            note.Priority = updateNote.Priority;
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload with includes
            var updatedNote = await _context.Notes
                .Include(n => n.Course)
                .Include(n => n.Lesson)
                .Include(n => n.User)
                .FirstAsync(n => n.Id == note.Id);

            return MapToDTO(updatedNote);
        }

        public async Task<bool> DeleteNoteAsync(int id, string userId)
        {
            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && !n.IsDeleted);

            if (note == null) return false;

            note.IsDeleted = true;
            note.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> TogglePinNoteAsync(int id, string userId)
        {
            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && !n.IsDeleted);

            if (note == null) return false;

            note.IsPinned = !note.IsPinned;
            note.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<NotesSummaryDTO> GetNotesSummaryAsync(string userId)
        {
            var notes = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .ToListAsync();

            var recentDate = DateTime.UtcNow.AddDays(-7);
            var tags = notes
                .Where(n => !string.IsNullOrEmpty(n.Tags))
                .SelectMany(n => n.Tags!.Split(',').Select(t => t.Trim()))
                .GroupBy(t => t.ToLower())
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            return new NotesSummaryDTO
            {
                TotalNotes = notes.Count,
                PinnedNotes = notes.Count(n => n.IsPinned),
                PrivateNotes = notes.Count(n => n.IsPrivate),
                PublicNotes = notes.Count(n => !n.IsPrivate),
                RecentNotes = notes.Count(n => n.CreatedAt >= recentDate),
                NotesByType = notes.GroupBy(n => n.Type).ToDictionary(g => g.Key, g => g.Count()),
                NotesByPriority = notes.GroupBy(n => n.Priority).ToDictionary(g => g.Key, g => g.Count()),
                MostUsedTags = tags
            };
        }

        public async Task<IEnumerable<NoteQuickAccessDTO>> GetPinnedNotesAsync(string userId)
        {
            var notes = await _context.Notes
                .Include(n => n.Course)
                .Where(n => n.UserId == userId && n.IsPinned && !n.IsDeleted)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync();

            return notes.Select(n => new NoteQuickAccessDTO
            {
                Id = n.Id,
                Title = n.Title,
                CourseTitle = n.Course?.Title,
                Type = n.Type,
                Priority = n.Priority,
                IsPinned = n.IsPinned,
                CreatedAt = n.CreatedAt
            });
        }

        public async Task<IEnumerable<NoteQuickAccessDTO>> GetRecentNotesAsync(string userId, int count = 5)
        {
            var notes = await _context.Notes
                .Include(n => n.Course)
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.UpdatedAt)
                .Take(count)
                .ToListAsync();

            return notes.Select(n => new NoteQuickAccessDTO
            {
                Id = n.Id,
                Title = n.Title,
                CourseTitle = n.Course?.Title,
                Type = n.Type,
                Priority = n.Priority,
                IsPinned = n.IsPinned,
                CreatedAt = n.CreatedAt
            });
        }

        public async Task<IEnumerable<string>> GetUserTagsAsync(string userId)
        {
            var notes = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsDeleted && !string.IsNullOrEmpty(n.Tags))
                .Select(n => n.Tags!)
                .ToListAsync();

            return notes
                .SelectMany(tags => tags.Split(',').Select(t => t.Trim()))
                .Distinct()
                .OrderBy(t => t)
                .ToList();
        }

        public async Task<IEnumerable<NoteDTO>> SearchNotesAsync(string userId, string searchTerm)
        {
            var filter = new NoteFilterDTO { SearchTerm = searchTerm };
            return await GetUserNotesAsync(userId, filter);
        }

        public async Task<IEnumerable<NoteDTO>> GetNotesForCourseAsync(string userId, int courseId)
        {
            var filter = new NoteFilterDTO { CourseId = courseId };
            return await GetUserNotesAsync(userId, filter);
        }

        public async Task<IEnumerable<NoteDTO>> GetNotesForLessonAsync(string userId, int lessonId)
        {
            var filter = new NoteFilterDTO { LessonId = lessonId };
            return await GetUserNotesAsync(userId, filter);
        }

        private static NoteDTO MapToDTO(Note note)
        {
            return new NoteDTO
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                UserId = note.UserId,
                UserName = note.User?.FullName ?? "",
                CourseId = note.CourseId,
                CourseTitle = note.Course?.Title,
                LessonId = note.LessonId,
                LessonTitle = note.Lesson?.Title,
                IsPrivate = note.IsPrivate,
                IsPinned = note.IsPinned,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt,
                Tags = note.Tags,
                Type = note.Type,
                Priority = note.Priority
            };
        }
    }
}
