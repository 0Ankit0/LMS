using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using LMS.Data.DTOs;
using LMS.Infrastructure.Data;

namespace LMS.Repositories
{
    public interface ICourseRepository
    {
        Task<List<CourseModel>> GetCoursesAsync();
        Task<PaginatedResult<CourseModel>> GetCoursesPaginatedAsync(PaginationRequest request);
        Task<CourseModel?> GetCourseByIdAsync(int id);
        Task<CourseModel> CreateCourseAsync(CreateCourseRequest request);
        Task<CourseModel> UpdateCourseAsync(int id, CreateCourseRequest request);
        Task<bool> DeleteCourseAsync(int id);
        Task<List<ModuleModel>> GetCourseModulesAsync(int courseId);
        Task<ModuleModel> CreateModuleAsync(CreateModuleRequest request);
        Task<List<LessonModel>> GetModuleLessonsAsync(int moduleId);
        Task<LessonModel> CreateLessonAsync(CreateLessonRequest request);
    }

    public class CourseRepository : ICourseRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public CourseRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<CourseModel>> GetCoursesAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            var courses = await context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .Include(c => c.Enrollments)
                .Include(c => c.CourseCategories)
                    .ThenInclude(cc => cc.Category)
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .ToListAsync();

            var courseModels = courses.Select(MapToCourseModel).ToList();

            // Add dummy data for demo purposes
            courseModels.AddRange(new[]
            {
        new CourseModel
        {
            Id = 1,
            Title = "Introduction to Quantum Computing",
            Description = "A beginner-friendly course on the basics of quantum computing and its applications.",
            InstructorId = "dummy1",
            InstructorName = "Dr. Alice Quantum",
            Level = "Advanced",
            Status = "Published",
            MaxEnrollments = 100,
            StartDate = DateTime.Today.AddDays(10),
            EndDate = DateTime.Today.AddDays(40),
            EstimatedDuration = TimeSpan.FromHours(20),
            Prerequisites = "Linear Algebra, Classical Computing",
            LearningObjectives = "Understand quantum bits, gates, and algorithms.",
            EnrollmentCount = 0,
            AverageRating = 0.0,
            Categories = new List<string> { "Computer Science", "Physics" },
            Tags = new List<string> { "Quantum", "Computing", "Advanced" }
        },
        new CourseModel
        {
            Id = 2,
            Title = "Creative Writing Workshop",
            Description = "Unlock your creativity and learn the art of storytelling in this interactive workshop.",
            InstructorId = "dummy2",
            InstructorName = "Prof. John Storyteller",
            Level = "Beginner",
            Status = "Draft",
            MaxEnrollments = 50,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(25),
            EstimatedDuration = TimeSpan.FromHours(10),
            Prerequisites = null,
            LearningObjectives = "Write compelling stories and improve narrative skills.",
            EnrollmentCount = 0,
            AverageRating = 0.0,
            Categories = new List<string> { "Arts", "Writing" },
            Tags = new List<string> { "Writing", "Creativity" }
        }
    });

            return courseModels;
        }

        public async Task<PaginatedResult<CourseModel>> GetCoursesPaginatedAsync(PaginationRequest request)
        {
            request.Validate();
            using var context = _contextFactory.CreateDbContext();
            var query = context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .Include(c => c.Enrollments)
                .Include(c => c.CourseCategories)
                    .ThenInclude(cc => cc.Category)
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .OrderBy(c => c.Title);

            var totalCount = await query.CountAsync();
            var skip = (request.PageNumber - 1) * request.PageSize;
            var courses = await query.Skip(skip).Take(request.PageSize).ToListAsync();

            return new PaginatedResult<CourseModel>
            {
                Items = courses.Select(MapToCourseModel).ToList(),
                TotalCount = totalCount,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };
        }

        public async Task<CourseModel?> GetCourseByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var course = await context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .Include(c => c.Enrollments)
                .Include(c => c.CourseCategories)
                    .ThenInclude(cc => cc.Category)
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .FirstOrDefaultAsync(c => c.Id == id);

            return course != null ? MapToCourseModel(course) : null;
        }

        public async Task<CourseModel> CreateCourseAsync(CreateCourseRequest request)
        {
            using var context = _contextFactory.CreateDbContext();
            var course = new Course
            {
                Title = request.Title,
                Description = request.Description,
                ThumbnailUrl = request.ThumbnailUrl,
                InstructorId = "current-user-id", // This should come from auth context
                Level = (CourseLevel)request.Level,
                Status = CourseStatus.Draft,
                MaxEnrollments = request.MaxEnrollments,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                EstimatedDuration = request.EstimatedDuration,
                Prerequisites = request.Prerequisites,
                LearningObjectives = request.LearningObjectives,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Courses.Add(course);
            await context.SaveChangesAsync();

            foreach (var categoryId in request.CategoryIds)
            {
                context.CourseCategories.Add(new CourseCategory
                {
                    CourseId = course.Id,
                    CategoryId = categoryId
                });
            }

            foreach (var tagId in request.TagIds)
            {
                context.CourseTags.Add(new CourseTags
                {
                    CourseId = course.Id,
                    TagId = tagId
                });
            }

            await context.SaveChangesAsync();

            // Use a new context to fetch the course with all includes
            return await GetCourseByIdAsync(course.Id) ?? throw new InvalidOperationException("Course not found after creation");
        }

        public async Task<CourseModel> UpdateCourseAsync(int id, CreateCourseRequest request)
        {
            using var context = _contextFactory.CreateDbContext();
            var course = await context.Courses.FindAsync(id);
            if (course == null)
                throw new ArgumentException("Course not found");

            course.Title = request.Title;
            course.Description = request.Description;
            course.ThumbnailUrl = request.ThumbnailUrl;
            course.Level = (CourseLevel)request.Level;
            course.MaxEnrollments = request.MaxEnrollments;
            course.StartDate = request.StartDate;
            course.EndDate = request.EndDate;
            course.EstimatedDuration = request.EstimatedDuration;
            course.Prerequisites = request.Prerequisites;
            course.LearningObjectives = request.LearningObjectives;
            course.UpdatedAt = DateTime.UtcNow;

            var existingCategories = await context.CourseCategories
                .Where(cc => cc.CourseId == id)
                .ToListAsync();
            context.CourseCategories.RemoveRange(existingCategories);

            foreach (var categoryId in request.CategoryIds)
            {
                context.CourseCategories.Add(new CourseCategory
                {
                    CourseId = id,
                    CategoryId = categoryId
                });
            }

            var existingTags = await context.CourseTags
                .Where(ct => ct.CourseId == id)
                .ToListAsync();
            context.CourseTags.RemoveRange(existingTags);

            foreach (var tagId in request.TagIds)
            {
                context.CourseTags.Add(new CourseTags
                {
                    CourseId = id,
                    TagId = tagId
                });
            }

            await context.SaveChangesAsync();

            return await GetCourseByIdAsync(id) ?? throw new InvalidOperationException("Course not found after update");
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var course = await context.Courses.FindAsync(id);
            if (course == null)
                return false;

            context.Courses.Remove(course);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ModuleModel>> GetCourseModulesAsync(int courseId)
        {
            using var context = _contextFactory.CreateDbContext();
            var modules = await context.Modules
                .Where(m => m.CourseId == courseId)
                .Include(m => m.Lessons)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();

            return modules.Select(MapToModuleModel).ToList();
        }

        public async Task<ModuleModel> CreateModuleAsync(CreateModuleRequest request)
        {
            using var context = _contextFactory.CreateDbContext();
            var module = new Module
            {
                Title = request.Title,
                Description = request.Description,
                CourseId = request.CourseId,
                OrderIndex = request.OrderIndex,
                IsRequired = request.IsRequired,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Modules.Add(module);
            await context.SaveChangesAsync();

            return MapToModuleModel(module);
        }

        public async Task<List<LessonModel>> GetModuleLessonsAsync(int moduleId)
        {
            using var context = _contextFactory.CreateDbContext();
            var lessons = await context.Lessons
                .Where(l => l.ModuleId == moduleId)
                .Include(l => l.Resources)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();

            return lessons.Select(MapToLessonModel).ToList();
        }

        public async Task<LessonModel> CreateLessonAsync(CreateLessonRequest request)
        {
            using var context = _contextFactory.CreateDbContext();
            var lesson = new Lesson
            {
                Title = request.Title,
                Description = request.Description,
                Content = request.Content,
                ModuleId = request.ModuleId,
                Type = (LessonType)request.Type,
                VideoUrl = request.VideoUrl,
                DocumentUrl = request.DocumentUrl,
                ExternalUrl = request.ExternalUrl,
                EstimatedDuration = request.EstimatedDuration,
                OrderIndex = request.OrderIndex,
                IsRequired = request.IsRequired,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Lessons.Add(lesson);
            await context.SaveChangesAsync();

            return MapToLessonModel(lesson);
        }

        private static CourseModel MapToCourseModel(Course course)
        {
            return new CourseModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                ThumbnailUrl = course.ThumbnailUrl,
                InstructorId = course.InstructorId,
                InstructorName = course.Instructor?.UserName ?? "Unknown",
                Level = course.Level.ToString(),
                Status = course.Status.ToString(),
                MaxEnrollments = course.MaxEnrollments,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                EstimatedDuration = course.EstimatedDuration,
                Prerequisites = course.Prerequisites,
                LearningObjectives = course.LearningObjectives,
                EnrollmentCount = course.Enrollments?.Count ?? 0,
                AverageRating = 0.0, // This would need to be calculated from reviews
                Categories = course.CourseCategories?.Select(cc => cc.Category.Name).ToList() ?? new List<string>(),
                Tags = course.CourseTags?.Select(ct => ct.Tag.Name).ToList() ?? new List<string>(),
                Modules = course.Modules?.Select(MapToModuleModel).ToList() ?? new List<ModuleModel>()
            };
        }

        private static ModuleModel MapToModuleModel(Module module)
        {
            return new ModuleModel
            {
                Id = module.Id,
                Title = module.Title,
                Description = module.Description,
                CourseId = module.CourseId,
                OrderIndex = module.OrderIndex,
                IsRequired = module.IsRequired,
                IsActive = module.IsActive,
                Lessons = module.Lessons?.Select(MapToLessonModel).ToList() ?? new List<LessonModel>(),
                ProgressPercentage = 0.0 // This would need to be calculated based on user progress
            };
        }

        private static LessonModel MapToLessonModel(Lesson lesson)
        {
            return new LessonModel
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Content = lesson.Content,
                ModuleId = lesson.ModuleId,
                Type = lesson.Type.ToString(),
                VideoUrl = lesson.VideoUrl,
                DocumentUrl = lesson.DocumentUrl,
                ExternalUrl = lesson.ExternalUrl,
                EstimatedDuration = lesson.EstimatedDuration,
                OrderIndex = lesson.OrderIndex,
                IsRequired = lesson.IsRequired,
                IsActive = lesson.IsActive,
                IsCompleted = false, // This would need to be calculated based on user progress
                ProgressPercentage = 0.0, // This would need to be calculated based on user progress
                Resources = lesson.Resources?.Select(r => new LessonResourceModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Type = r.Type.ToString(),
                    FilePath = r.FilePath,
                    ExternalUrl = r.ExternalUrl,
                    FileSize = r.FileSize,
                    ContentType = r.ContentType,
                    IsDownloadable = r.IsDownloadable
                }).ToList() ?? new List<LessonResourceModel>()
            };
        }
    }
}
