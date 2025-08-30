using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using LMS.Data.DTOs;
using LMS.Web.Data;

namespace LMS.Repositories
{
    public interface ICourseRepository
    {
        Task<List<CourseModel>> GetCoursesAsync();
        Task<PaginatedResult<CourseModel>> GetCoursesAsync(GetCoursesRequest request);
        Task<PaginatedResult<CourseModel>> GetCoursesPaginatedAsync(PaginationRequest request);
        Task<CourseModel?> GetCourseByIdAsync(int id);
        Task<CourseModel?> GetByIdAsync(int id); // Alias for GetCourseByIdAsync
        Task<CourseModel> CreateCourseAsync(CreateCourseRequest request);
        Task<CourseModel> UpdateCourseAsync(int id, UpdateCourseRequest request);
        Task<bool> DeleteCourseAsync(int id);
        Task<List<ModuleModel>> GetCourseModulesAsync(int courseId);
        Task<ModuleModel> CreateModuleAsync(CreateModuleRequest request);
        Task<ModuleModel?> GetModuleByIdAsync(int id);
        Task<ModuleModel> UpdateModuleAsync(int id, CreateModuleRequest request);
        Task<bool> DeleteModuleAsync(int id);
        Task<List<LessonModel>> GetModuleLessonsAsync(int moduleId);
        Task<LessonModel> CreateLessonAsync(CreateLessonRequest request);
        Task<LessonModel?> GetLessonByIdAsync(int id);
        Task<LessonModel> UpdateLessonAsync(int id, CreateLessonRequest request);
        Task<bool> DeleteLessonAsync(int id);
        Task<List<CourseModel>> GetAllCoursesAsync();
        Task<PaginatedResult<ModuleModel>> GetModulesPaginatedAsync(PaginationRequest request);
        Task<List<CourseModel>> GetCoursesByCategoryAsync(int categoryId);
        Task<List<CourseModel>> GetFeaturedCoursesAsync(int limit = 10);
        Task<List<CourseModel>> GetStudentCoursesAsync(int studentId);
        Task<List<CourseModel>> GetPopularCoursesAsync(int limit = 10);
    }

    public class CourseRepository : ICourseRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<CourseRepository> _logger;

        public CourseRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<CourseRepository> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<CourseModel>> GetCoursesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var courses = await context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.ThumbnailFile)
                    .Include(c => c.Modules)
                    .Include(c => c.Enrollments)
                    .Include(c => c.CourseCategories)
                        .ThenInclude(cc => cc.Category)
                    .Include(c => c.CourseTags)
                        .ThenInclude(ct => ct.Tag)
                    .ToListAsync();
                return courses.Select(MapToCourseModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses");
                throw;
            }
        }

        public async Task<PaginatedResult<CourseModel>> GetCoursesAsync(GetCoursesRequest request)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var query = context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.ThumbnailFile)
                    .Include(c => c.CourseCategories)
                        .ThenInclude(cc => cc.Category)
                    .Include(c => c.CourseTags)
                        .ThenInclude(ct => ct.Tag)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(c => c.Title.Contains(request.Search) ||
                                           c.Description.Contains(request.Search));
                }

                if (request.CategoryId.HasValue)
                {
                    query = query.Where(c => c.CourseCategories.Any(cc => cc.CategoryId == request.CategoryId.Value));
                }

                if (!string.IsNullOrEmpty(request.Level))
                {
                    if (Enum.TryParse<CourseLevel>(request.Level, true, out var level))
                    {
                        query = query.Where(c => c.Level == level);
                    }
                }

                if (request.IsActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == request.IsActive.Value);
                }

                if (!string.IsNullOrEmpty(request.InstructorId))
                {
                    query = query.Where(c => c.InstructorId == request.InstructorId);
                }

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "title" => request.SortDirection?.ToLower() == "desc" ?
                               query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title),
                    "created" => request.SortDirection?.ToLower() == "desc" ?
                                query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                    _ => query.OrderBy(c => c.Title)
                };

                var totalCount = await query.CountAsync();
                var courses = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return new PaginatedResult<CourseModel>
                {
                    Items = courses.Select(MapToCourseModel).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.Page,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses with filters");
                throw;
            }
        }

        public async Task<PaginatedResult<CourseModel>> GetCoursesPaginatedAsync(PaginationRequest request)
        {
            try
            {
                request.Validate();
                using var context = _contextFactory.CreateDbContext();
                var query = context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.ThumbnailFile)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated courses");
                throw;
            }
        }

        public async Task<CourseModel?> GetCourseByIdAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var course = await context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.ThumbnailFile)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course by id: {CourseId}", id);
                throw;
            }
        }

        public async Task<CourseModel> CreateCourseAsync(CreateCourseRequest request)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var course = new Course
                {
                    Title = request.Title,
                    Description = request.Description,
                    InstructorId = "current-user-id", // This should come from auth context
                    Level = Enum.TryParse<CourseLevel>(request.Level, true, out var levelEnum) ? levelEnum : CourseLevel.Beginner,
                    Status = CourseStatus.Draft,
                    MaxEnrollments = 0, // Default to unlimited
                    StartDate = DateTime.UtcNow, // Default to now
                    EndDate = null,
                    EstimatedDuration = TimeSpan.Zero, // Default to zero
                    Prerequisites = request.Prerequisites != null ? string.Join(",", request.Prerequisites) : null,
                    LearningObjectives = request.LearningObjectives != null ? string.Join(",", request.LearningObjectives) : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Courses.Add(course);
                await context.SaveChangesAsync();

                // Add course category (single category from request)
                context.CourseCategories.Add(new CourseCategory
                {
                    CourseId = course.Id,
                    CategoryId = request.CategoryId
                });

                // Add course tags if provided
                if (request.Tags != null && request.Tags.Any())
                {
                    // For now, we'll create tags on-the-fly if they don't exist
                    // In a real implementation, you might want to validate against existing tags
                    foreach (var tagName in request.Tags)
                    {
                        var existingTag = await context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                        if (existingTag == null)
                        {
                            existingTag = new Tag { Name = tagName };
                            context.Tags.Add(existingTag);
                            await context.SaveChangesAsync();
                        }

                        context.CourseTags.Add(new CourseTags
                        {
                            CourseId = course.Id,
                            TagId = existingTag.Id
                        });
                    }
                }

                await context.SaveChangesAsync();

                // Use a new context to fetch the course with all includes
                return await GetCourseByIdAsync(course.Id) ?? throw new InvalidOperationException("Course not found after creation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                throw;
            }
        }

        public async Task<CourseModel> UpdateCourseAsync(int id, UpdateCourseRequest request)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var course = await context.Courses.FindAsync(id);
                if (course == null)
                    throw new ArgumentException("Course not found");

                if (request.Title != null) course.Title = request.Title;
                if (request.Description != null) course.Description = request.Description;
                if (request.Level != null) course.Level = Enum.TryParse<CourseLevel>(request.Level, true, out var levelEnum) ? levelEnum : course.Level;
                // MaxEnrollments, StartDate, EndDate, EstimatedDuration are not in UpdateCourseRequest
                if (request.Prerequisites != null) course.Prerequisites = string.Join(",", request.Prerequisites);
                if (request.LearningObjectives != null) course.LearningObjectives = string.Join(",", request.LearningObjectives);
                course.UpdatedAt = DateTime.UtcNow;

                // Update categories if CategoryId is provided
                if (request.CategoryId.HasValue)
                {
                    var existingCategories = await context.CourseCategories
                        .Where(cc => cc.CourseId == id)
                        .ToListAsync();
                    context.CourseCategories.RemoveRange(existingCategories);

                    context.CourseCategories.Add(new CourseCategory
                    {
                        CourseId = id,
                        CategoryId = request.CategoryId.Value
                    });
                }

                // Update tags if provided
                if (request.Tags != null)
                {
                    var existingTags = await context.CourseTags
                        .Where(ct => ct.CourseId == id)
                        .ToListAsync();
                    context.CourseTags.RemoveRange(existingTags);

                    foreach (var tagName in request.Tags)
                    {
                        var existingTag = await context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                        if (existingTag == null)
                        {
                            existingTag = new Tag { Name = tagName };
                            context.Tags.Add(existingTag);
                            await context.SaveChangesAsync();
                        }

                        context.CourseTags.Add(new CourseTags
                        {
                            CourseId = id,
                            TagId = existingTag.Id
                        });
                    }
                }

                await context.SaveChangesAsync();

                return await GetCourseByIdAsync(id) ?? throw new InvalidOperationException("Course not found after update");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course: {CourseId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var course = await context.Courses.FindAsync(id);
                if (course == null)
                    return false;

                context.Courses.Remove(course);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course: {CourseId}", id);
                throw;
            }
        }

        public async Task<List<ModuleModel>> GetCourseModulesAsync(int courseId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var modules = await context.Modules
                    .Where(m => m.CourseId == courseId)
                    .Include(m => m.Lessons)
                    .OrderBy(m => m.OrderIndex)
                    .ToListAsync();

                return modules.Select(MapToModuleModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting modules for course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<ModuleModel> CreateModuleAsync(CreateModuleRequest request)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating module for course: {CourseId}", request.CourseId);
                throw;
            }
        }

        public async Task<ModuleModel?> GetModuleByIdAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var module = await context.Modules
                    .Include(m => m.Lessons)
                    .FirstOrDefaultAsync(m => m.Id == id);

                return module != null ? MapToModuleModel(module) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting module by id: {ModuleId}", id);
                throw;
            }
        }

        public async Task<ModuleModel> UpdateModuleAsync(int id, CreateModuleRequest request)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var module = await context.Modules.FindAsync(id);
                if (module == null)
                    throw new ArgumentException("Module not found");

                module.Title = request.Title;
                module.Description = request.Description;
                module.OrderIndex = request.OrderIndex;
                module.IsRequired = request.IsRequired;
                module.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return await GetModuleByIdAsync(id) ?? throw new InvalidOperationException("Module not found after update");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating module: {ModuleId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteModuleAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var module = await context.Modules.FindAsync(id);
                if (module == null)
                    return false;

                context.Modules.Remove(module);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting module: {ModuleId}", id);
                throw;
            }
        }

        public async Task<List<LessonModel>> GetModuleLessonsAsync(int moduleId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var lessons = await context.Lessons
                    .Where(l => l.ModuleId == moduleId)
                    .Include(l => l.Resources)
                    .OrderBy(l => l.OrderIndex)
                    .ToListAsync();

                return lessons.Select(MapToLessonModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lessons for module: {ModuleId}", moduleId);
                throw;
            }
        }

        public async Task<LessonModel> CreateLessonAsync(CreateLessonRequest request)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lesson for module: {ModuleId}", request.ModuleId);
                throw;
            }
        }

        public async Task<LessonModel?> GetLessonByIdAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var lesson = await context.Lessons
                    .Include(l => l.Resources)
                    .FirstOrDefaultAsync(l => l.Id == id);

                return lesson != null ? MapToLessonModel(lesson) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson by id: {LessonId}", id);
                throw;
            }
        }

        public async Task<LessonModel> UpdateLessonAsync(int id, CreateLessonRequest request)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var lesson = await context.Lessons.FindAsync(id);
                if (lesson == null)
                    throw new ArgumentException("Lesson not found");

                lesson.Title = request.Title;
                lesson.Description = request.Description;
                lesson.Content = request.Content;
                lesson.VideoUrl = request.VideoUrl;
                lesson.DocumentUrl = request.DocumentUrl;
                lesson.ExternalUrl = request.ExternalUrl;
                lesson.EstimatedDuration = request.EstimatedDuration;
                lesson.OrderIndex = request.OrderIndex;
                lesson.IsRequired = request.IsRequired;
                lesson.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return await GetLessonByIdAsync(id) ?? throw new InvalidOperationException("Lesson not found after update");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lesson: {LessonId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteLessonAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var lesson = await context.Lessons.FindAsync(id);
                if (lesson == null)
                    return false;

                context.Lessons.Remove(lesson);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lesson: {LessonId}", id);
                throw;
            }
        }

        public async Task<List<CourseModel>> GetAllCoursesAsync()
        {
            // For now, just call GetCoursesAsync
            return await GetCoursesAsync();
        }

        public async Task<PaginatedResult<ModuleModel>> GetModulesPaginatedAsync(PaginationRequest request)
        {
            try
            {
                request.Validate();
                using var context = _contextFactory.CreateDbContext();
                var query = context.Modules
                    .Include(m => m.Lessons)
                    .OrderBy(m => m.CourseId)
                    .ThenBy(m => m.OrderIndex);

                var totalCount = await query.CountAsync();
                var skip = (request.PageNumber - 1) * request.PageSize;
                var modules = await query.Skip(skip).Take(request.PageSize).ToListAsync();

                return new PaginatedResult<ModuleModel>
                {
                    Items = modules.Select(MapToModuleModel).ToList(),
                    TotalCount = totalCount,
                    PageSize = request.PageSize,
                    PageNumber = request.PageNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated modules");
                throw;
            }
        }

        public async Task<List<CourseModel>> GetCoursesByCategoryAsync(int categoryId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var courses = await context.Courses
                    .Include(c => c.CourseCategories)
                        .ThenInclude(cc => cc.Category)
                    .Include(c => c.ThumbnailFile)
                    .Include(c => c.Instructor)
                    .Where(c => c.CourseCategories.Any(cc => cc.CategoryId == categoryId) && c.IsActive)
                    .OrderBy(c => c.Title)
                    .ToListAsync();

                return courses.Select(MapToCourseModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses by category: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<List<CourseModel>> GetFeaturedCoursesAsync(int limit = 10)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var courses = await context.Courses
                    .Include(c => c.CourseCategories)
                        .ThenInclude(cc => cc.Category)
                    .Include(c => c.ThumbnailFile)
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .Where(c => c.Status == CourseStatus.Published && c.IsActive)
                    .OrderByDescending(c => c.Enrollments.Count) // Order by popularity as a proxy for "featured"
                    .Take(limit)
                    .ToListAsync();

                return courses.Select(MapToCourseModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured courses");
                throw;
            }
        }

        public async Task<List<CourseModel>> GetPopularCoursesAsync(int limit = 10)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var courses = await context.Courses
                    .Include(c => c.CourseCategories)
                        .ThenInclude(cc => cc.Category)
                    .Include(c => c.ThumbnailFile)
                    .Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.Enrollments.Count)
                    .Take(limit)
                    .ToListAsync();

                return courses.Select(MapToCourseModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular courses");
                throw;
            }
        }

        private static CourseModel MapToCourseModel(Course course)
        {
            return new CourseModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                ThumbnailUrl = course.ThumbnailFile?.FilePath ?? string.Empty,
                InstructorId = course.InstructorId,
                InstructorName = course.Instructor?.UserName ?? "Unknown",
                Level = course.Level.ToString(),
                Status = course.Status.ToString(),
                MaxEnrollments = course.MaxEnrollments,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                EstimatedDuration = (int?)course.EstimatedDuration.TotalMinutes,
                Prerequisites = string.IsNullOrEmpty(course.Prerequisites) ? new List<string>() : course.Prerequisites.Split(',').ToList(),
                LearningObjectives = string.IsNullOrEmpty(course.LearningObjectives) ? new List<string>() : course.LearningObjectives.Split(',').ToList(),
                EnrollmentCount = course.Enrollments?.Count ?? 0,
                AverageRating = 0.0m, // This would need to be calculated from reviews
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
                Duration = module.Lessons?.Sum(l => (int)l.EstimatedDuration.TotalMinutes) ?? 0, // Sum of lesson durations in minutes
                Lessons = module.Lessons?.Select(MapToLessonModel).ToList() ?? new List<LessonModel>()
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
                Type = lesson.Type.ToString(),
                ContentUrl = lesson.VideoUrl ?? lesson.DocumentUrl ?? lesson.ExternalUrl, // Map to ContentUrl
                Duration = (int)lesson.EstimatedDuration.TotalMinutes, // Convert TimeSpan to minutes
                OrderIndex = lesson.OrderIndex,
                IsActive = lesson.IsActive,
                IsCompleted = false, // This would need to be calculated based on user progress
                CompletedAt = null
            };
        }

        public async Task<List<CourseModel>> GetStudentCoursesAsync(int studentId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                // Get courses the student is enrolled in
                var enrollments = await context.Enrollments
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Instructor)
                    .Include(e => e.Course)
                        .ThenInclude(c => c.CourseCategories)
                            .ThenInclude(cc => cc.Category)
                    .Include(e => e.Course)
                        .ThenInclude(c => c.CourseTags)
                            .ThenInclude(ct => ct.Tag)
                    .Where(e => e.UserId == studentId.ToString())
                    .ToListAsync();

                var courses = new List<CourseModel>();
                foreach (var enrollment in enrollments)
                {
                    var course = MapToCourseModel(enrollment.Course);
                    course.IsCompleted = enrollment.Status == EnrollmentStatus.Completed;
                    course.ProgressPercentage = (int)enrollment.ProgressPercentage;
                    course.IsEnrolled = true;
                    courses.Add(course);
                }

                return courses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student courses for student {StudentId}", studentId);
                return new List<CourseModel>();
            }
        }

        public async Task<CourseModel?> GetByIdAsync(int id)
        {
            // Alias for GetCourseByIdAsync
            return await GetCourseByIdAsync(id);
        }
    }
}
