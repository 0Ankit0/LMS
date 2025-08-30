using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Infrastructure;
using LMS.Web.Repositories;
using LMS.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LMS.Web.Endpoints
{
    public class CourseEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/courses").WithTags("Courses");

            // Course management
            group.MapGet("", GetCourses)
                .WithName("GetCourses")
                .WithSummary("Get all courses");

            group.MapGet("/{id:int}", GetCourseById)
                .WithName("GetCourseById")
                .WithSummary("Get course by ID");

            group.MapPost("", CreateCourse)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("CreateCourse")
                .WithSummary("Create a new course");

            group.MapPut("/{id:int}", UpdateCourse)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("UpdateCourse")
                .WithSummary("Update an existing course");

            group.MapDelete("/{id:int}", DeleteCourse)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("DeleteCourse")
                .WithSummary("Delete a course");

            // Course enrollment
            group.MapPost("/{id:int}/enroll", EnrollInCourse)
                .RequireAuthorization()
                .WithName("EnrollInCourse")
                .WithSummary("Enroll current user in a course");

            group.MapDelete("/{id:int}/unenroll", UnenrollFromCourse)
                .RequireAuthorization()
                .WithName("UnenrollFromCourse")
                .WithSummary("Unenroll current user from a course");

            group.MapGet("/{id:int}/enrollments", GetCourseEnrollments)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("GetCourseEnrollments")
                .WithSummary("Get all enrollments for a course");

            // Course content
            group.MapGet("/{id:int}/modules", GetCourseModules)
                .RequireAuthorization()
                .WithName("GetCourseModules")
                .WithSummary("Get all modules for a course");

            group.MapGet("/{id:int}/lessons", GetCourseLessons)
                .RequireAuthorization()
                .WithName("GetCourseLessons")
                .WithSummary("Get all lessons for a course");

            // Course progress
            group.MapGet("/{id:int}/progress", GetCourseProgress)
                .RequireAuthorization()
                .WithName("GetCourseProgress")
                .WithSummary("Get user's progress in a course");

            group.MapPost("/{id:int}/complete", CompleteCourse)
                .RequireAuthorization()
                .WithName("CompleteCourse")
                .WithSummary("Mark course as completed");

            // Course categories and filtering
            group.MapGet("/category/{categoryId:int}", GetCoursesByCategory)
                .WithName("GetCoursesByCategory")
                .WithSummary("Get courses by category");

            group.MapGet("/featured", GetFeaturedCourses)
                .WithName("GetFeaturedCourses")
                .WithSummary("Get featured courses");

            group.MapGet("/popular", GetPopularCourses)
                .WithName("GetPopularCourses")
                .WithSummary("Get popular courses");
        }

        private static async Task<IResult> GetCourses(
            ICourseRepository courseRepository,
            string? search = null,
            int? categoryId = null,
            string? level = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var request = new GetCoursesRequest
                {
                    Search = search,
                    CategoryId = categoryId,
                    Level = level,
                    IsActive = isActive,
                    Page = page,
                    PageSize = pageSize
                };

                var courses = await courseRepository.GetCoursesAsync(request);
                return Results.Ok(courses);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving courses: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCourseById(
            int id,
            ICourseRepository courseRepository,
            ClaimsPrincipal? user = null)
        {
            try
            {
                var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var course = await courseRepository.GetCourseByIdAsync(id);

                if (course == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(course);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving course: {ex.Message}");
            }
        }

        private static async Task<IResult> CreateCourse(
            CreateCourseRequest request,
            ICourseRepository courseRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var course = await courseRepository.CreateCourseAsync(request);
                return Results.CreatedAtRoute("GetCourseById", new { id = course.Id }, course);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating course: {ex.Message}");
            }
        }

        private static async Task<IResult> UpdateCourse(
            int id,
            UpdateCourseRequest request,
            ICourseRepository courseRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var course = await courseRepository.UpdateCourseAsync(id, request);
                if (course == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(course);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating course: {ex.Message}");
            }
        }

        private static async Task<IResult> DeleteCourse(
            int id,
            ICourseRepository courseRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var success = await courseRepository.DeleteCourseAsync(id);
                if (!success)
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting course: {ex.Message}");
            }
        }

        private static async Task<IResult> EnrollInCourse(
            int id,
            IEnrollmentRepository enrollmentRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var enrollment = await enrollmentRepository.EnrollUserAsync(userId, id);
                return Results.Ok(enrollment);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error enrolling in course: {ex.Message}");
            }
        }

        private static async Task<IResult> UnenrollFromCourse(
            int id,
            IEnrollmentRepository enrollmentRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var success = await enrollmentRepository.UnenrollUserAsync(userId, id);
                if (!success)
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error unenrolling from course: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCourseEnrollments(
            int id,
            IEnrollmentRepository enrollmentRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = user.IsInRole("Admin");

                // Check if user can view enrollments for this course
                if (!isAdmin)
                {
                    var isInstructor = await enrollmentRepository.IsInstructorOfCourseAsync(userId, id);
                    if (!isInstructor)
                    {
                        return Results.Forbid();
                    }
                }

                var enrollments = await enrollmentRepository.GetCourseEnrollmentsAsync(id);
                return Results.Ok(enrollments);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving course enrollments: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCourseModules(
            int id,
            IModuleRepository moduleRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var modules = await moduleRepository.GetModulesByCourseIdAsync(id);
                return Results.Ok(modules);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving course modules: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCourseLessons(
            int id,
            ILessonRepository lessonRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var lessons = await lessonRepository.GetCourseLessonsAsync(id);
                return Results.Ok(lessons);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving course lessons: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCourseProgress(
            int id,
            IProgressRepository progressRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var progress = await progressRepository.GetCourseProgressAsync(userId, id);
                return Results.Ok(progress);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving course progress: {ex.Message}");
            }
        }

        private static async Task<IResult> CompleteCourse(
            int id,
            IProgressRepository progressRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var result = await progressRepository.CompleteCourseAsync(userId, id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error completing course: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCoursesByCategory(
            int categoryId,
            ICourseRepository courseRepository)
        {
            try
            {
                var courses = await courseRepository.GetCoursesByCategoryAsync(categoryId);
                return Results.Ok(courses);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving courses by category: {ex.Message}");
            }
        }

        private static async Task<IResult> GetFeaturedCourses(
            ICourseRepository courseRepository,
            int limit = 10)
        {
            try
            {
                var courses = await courseRepository.GetFeaturedCoursesAsync(limit);
                return Results.Ok(courses);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving featured courses: {ex.Message}");
            }
        }

        private static async Task<IResult> GetPopularCourses(
            ICourseRepository courseRepository,
            int limit = 10)
        {
            try
            {
                var courses = await courseRepository.GetPopularCoursesAsync(limit);
                return Results.Ok(courses);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving popular courses: {ex.Message}");
            }
        }
    }
}
