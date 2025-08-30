using System.Security.Claims;
using LMS.Web.DTOs.UserManagement;
using LMS.Data.Entities;
using LMS.Web.Data;
using LMS.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.Web.Endpoints
{
    public class UserManagementEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/users").WithTags("User Management");

            // Student profile endpoints
            group.MapGet("/students/{userId}", GetStudentProfile)
                .RequireAuthorization()
                .WithName("GetStudentProfile")
                .WithSummary("Get student profile by user ID");

            group.MapPost("/students/{userId}", CreateStudentProfile)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("CreateStudentProfile")
                .WithSummary("Create a student profile for a user");

            group.MapPut("/students/{userId}", UpdateStudentProfile)
                .RequireAuthorization()
                .WithName("UpdateStudentProfile")
                .WithSummary("Update student profile");

            // Instructor profile endpoints
            group.MapGet("/instructors/{userId}", GetInstructorProfile)
                .RequireAuthorization()
                .WithName("GetInstructorProfile")
                .WithSummary("Get instructor profile by user ID");

            group.MapPost("/instructors/{userId}", CreateInstructorProfile)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("CreateInstructorProfile")
                .WithSummary("Create an instructor profile for a user");

            // Parent profile endpoints
            group.MapGet("/parents/{userId}", GetParentProfile)
                .RequireAuthorization()
                .WithName("GetParentProfile")
                .WithSummary("Get parent profile by user ID");

            group.MapPost("/parents/{userId}", CreateParentProfile)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("CreateParentProfile")
                .WithSummary("Create a parent profile for a user");

            // Parent-Student relationship endpoints
            group.MapPost("/parent-student-links", LinkParentStudent)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("LinkParentStudent")
                .WithSummary("Link a parent to a student");

            group.MapDelete("/parent-student-links/{parentId}/{studentId}", UnlinkParentStudent)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("UnlinkParentStudent")
                .WithSummary("Unlink a parent from a student");

            // User settings endpoints
            group.MapGet("/settings/{userId}", GetUserSettings)
                .RequireAuthorization()
                .WithName("GetUserSettings")
                .WithSummary("Get user settings");

            group.MapPut("/settings/{userId}", UpdateUserSettings)
                .RequireAuthorization()
                .WithName("UpdateUserSettings")
                .WithSummary("Update user settings");

            // User activity endpoints
            group.MapGet("/activities/{userId}", GetUserActivities)
                .RequireAuthorization()
                .WithName("GetUserActivities")
                .WithSummary("Get user activity log");

            group.MapPost("/activities", LogUserActivity)
                .RequireAuthorization()
                .WithName("LogUserActivity")
                .WithSummary("Log a user activity");
        }

        private static async Task<IResult> GetStudentProfile(string userId, ApplicationDbContext context)
        {
            var student = await context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return Results.NotFound();

            var model = new StudentProfileModel
            {
                UserId = student.UserId,
                DateOfBirth = student.DateOfBirth,
                StudentIdNumber = student.StudentIdNumber,
                EnrollmentDate = student.EnrollmentDate,
                GraduationDate = student.GraduationDate,
                EmergencyContactName = student.EmergencyContactName,
                EmergencyContactPhone = student.EmergencyContactPhone,
                TotalPoints = student.TotalPoints,
                Level = student.Level,
                FirstName = student.User.FirstName,
                LastName = student.User.LastName,
                Email = student.User.Email ?? string.Empty,
                Bio = student.User.Bio,
                ProfilePictureUrl = string.Empty // TODO: Implement profile picture file support when database schema is ready
            };

            return Results.Ok(model);
        }

        private static async Task<IResult> CreateStudentProfile(
            string userId,
            CreateStudentProfileRequest request,
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound("User not found");

            var existingProfile = await context.Students.FindAsync(userId);
            if (existingProfile != null)
                return Results.Conflict("Student profile already exists");

            var student = new Student
            {
                UserId = userId,
                DateOfBirth = request.DateOfBirth,
                StudentIdNumber = request.StudentIdNumber,
                EnrollmentDate = request.EnrollmentDate,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone,
                TotalPoints = 0,
                Level = 1
            };

            context.Students.Add(student);

            // Add Student role
            await userManager.AddToRoleAsync(user, "Student");

            await context.SaveChangesAsync();

            return Results.Created($"/api/users/students/{userId}", new { userId });
        }

        private static async Task<IResult> UpdateStudentProfile(
            string userId,
            UpdateStudentProfileRequest request,
            ApplicationDbContext context,
            ClaimsPrincipal user)
        {
            // Check if user can update this profile (admin or own profile)
            if (!user.IsInRole("Admin") && user.FindFirst("sub")?.Value != userId)
                return Results.Forbid();

            var student = await context.Students.FindAsync(userId);
            if (student == null)
                return Results.NotFound();

            if (request.DateOfBirth.HasValue)
                student.DateOfBirth = request.DateOfBirth.Value;

            if (!string.IsNullOrEmpty(request.StudentIdNumber))
                student.StudentIdNumber = request.StudentIdNumber;

            if (request.EnrollmentDate.HasValue)
                student.EnrollmentDate = request.EnrollmentDate;

            if (request.GraduationDate.HasValue)
                student.GraduationDate = request.GraduationDate;

            if (!string.IsNullOrEmpty(request.EmergencyContactName))
                student.EmergencyContactName = request.EmergencyContactName;

            if (!string.IsNullOrEmpty(request.EmergencyContactPhone))
                student.EmergencyContactPhone = request.EmergencyContactPhone;

            await context.SaveChangesAsync();

            return Results.NoContent();
        }

        private static async Task<IResult> GetInstructorProfile(string userId, ApplicationDbContext context)
        {
            var instructor = await context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.UserId == userId);

            if (instructor == null)
                return Results.NotFound();

            var model = new InstructorProfileModel
            {
                UserId = instructor.UserId,
                Department = instructor.Department,
                OfficeHours = instructor.OfficeHours,
                HireDate = instructor.HireDate,
                FirstName = instructor.User.FirstName,
                LastName = instructor.User.LastName,
                Email = instructor.User.Email ?? string.Empty,
                Bio = instructor.User.Bio,
                ProfilePictureUrl = string.Empty // TODO: Implement profile picture file support when database schema is ready
            };

            return Results.Ok(model);
        }

        private static async Task<IResult> CreateInstructorProfile(
            string userId,
            CreateInstructorProfileRequest request,
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound("User not found");

            var existingProfile = await context.Instructors.FindAsync(userId);
            if (existingProfile != null)
                return Results.Conflict("Instructor profile already exists");

            var instructor = new Instructor
            {
                UserId = userId,
                Department = request.Department,
                OfficeHours = request.OfficeHours,
                HireDate = request.HireDate
            };

            context.Instructors.Add(instructor);

            // Add Instructor role
            await userManager.AddToRoleAsync(user, "Instructor");

            await context.SaveChangesAsync();

            return Results.Created($"/api/users/instructors/{userId}", new { userId });
        }

        private static async Task<IResult> GetParentProfile(string userId, ApplicationDbContext context)
        {
            var parent = await context.Parents
                .Include(p => p.User)
                .Include(p => p.StudentLinks)
                .ThenInclude(sl => sl.Student)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (parent == null)
                return Results.NotFound();

            var model = new ParentProfileModel
            {
                UserId = parent.UserId,
                PreferredContactMethod = parent.PreferredContactMethod,
                FirstName = parent.User.FirstName,
                LastName = parent.User.LastName,
                Email = parent.User.Email ?? string.Empty,
                Bio = parent.User.Bio,
                ProfilePictureUrl = string.Empty, // TODO: Implement profile picture file support when database schema is ready
                LinkedStudents = parent.StudentLinks.Select(sl => new StudentProfileModel
                {
                    UserId = sl.Student.UserId,
                    FirstName = sl.Student.User.FirstName,
                    LastName = sl.Student.User.LastName,
                    Email = sl.Student.User.Email ?? string.Empty,
                    DateOfBirth = sl.Student.DateOfBirth,
                    StudentIdNumber = sl.Student.StudentIdNumber,
                    TotalPoints = sl.Student.TotalPoints,
                    Level = sl.Student.Level
                }).ToList()
            };

            return Results.Ok(model);
        }

        private static async Task<IResult> CreateParentProfile(
            string userId,
            CreateParentProfileRequest request,
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound("User not found");

            var existingProfile = await context.Parents.FindAsync(userId);
            if (existingProfile != null)
                return Results.Conflict("Parent profile already exists");

            var parent = new Parent
            {
                UserId = userId,
                PreferredContactMethod = request.PreferredContactMethod
            };

            context.Parents.Add(parent);

            // Add Parent role
            await userManager.AddToRoleAsync(user, "Parent");

            await context.SaveChangesAsync();

            return Results.Created($"/api/users/parents/{userId}", new { userId });
        }

        private static async Task<IResult> LinkParentStudent(
            LinkParentStudentRequest request,
            ApplicationDbContext context)
        {
            var parent = await context.Parents.FindAsync(request.ParentId);
            if (parent == null)
                return Results.NotFound("Parent not found");

            var student = await context.Students.FindAsync(request.StudentId);
            if (student == null)
                return Results.NotFound("Student not found");

            var existingLink = await context.ParentStudentLinks
                .FindAsync(request.ParentId, request.StudentId);
            if (existingLink != null)
                return Results.Conflict("Link already exists");

            var link = new ParentStudentLink
            {
                ParentId = request.ParentId,
                StudentId = request.StudentId
            };

            context.ParentStudentLinks.Add(link);
            await context.SaveChangesAsync();

            return Results.Created($"/api/users/parent-student-links/{request.ParentId}/{request.StudentId}", link);
        }

        private static async Task<IResult> UnlinkParentStudent(
            string parentId,
            string studentId,
            ApplicationDbContext context)
        {
            var link = await context.ParentStudentLinks
                .FindAsync(parentId, studentId);

            if (link == null)
                return Results.NotFound();

            context.ParentStudentLinks.Remove(link);
            await context.SaveChangesAsync();

            return Results.NoContent();
        }

        private static async Task<IResult> GetUserSettings(string userId, ApplicationDbContext context)
        {
            var settings = await context.UserSettings.FindAsync(userId);
            if (settings == null)
            {
                // Return default settings
                var defaultSettings = new UserSettingsModel
                {
                    UserId = userId,
                    Theme = "Light",
                    Language = "en-US",
                    EmailNotifications = true,
                    SmsNotifications = false,
                    PushNotifications = true
                };
                return Results.Ok(defaultSettings);
            }

            var model = new UserSettingsModel
            {
                UserId = settings.UserId,
                Theme = settings.Theme,
                Language = settings.Language,
                EmailNotifications = settings.EmailNotifications,
                SmsNotifications = settings.SmsNotifications,
                PushNotifications = settings.PushNotifications
            };

            return Results.Ok(model);
        }

        private static async Task<IResult> UpdateUserSettings(
            string userId,
            UpdateUserSettingsRequest request,
            ApplicationDbContext context,
            ClaimsPrincipal user)
        {
            // Check if user can update these settings (admin or own settings)
            if (!user.IsInRole("Admin") && user.FindFirst("sub")?.Value != userId)
                return Results.Forbid();

            var settings = await context.UserSettings.FindAsync(userId);
            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = userId,
                    Theme = request.Theme ?? "Light",
                    Language = request.Language ?? "en-US",
                    EmailNotifications = request.EmailNotifications ?? true,
                    SmsNotifications = request.SmsNotifications ?? false,
                    PushNotifications = request.PushNotifications ?? true
                };
                context.UserSettings.Add(settings);
            }
            else
            {
                if (request.Theme != null)
                    settings.Theme = request.Theme;
                if (request.Language != null)
                    settings.Language = request.Language;
                if (request.EmailNotifications.HasValue)
                    settings.EmailNotifications = request.EmailNotifications.Value;
                if (request.SmsNotifications.HasValue)
                    settings.SmsNotifications = request.SmsNotifications.Value;
                if (request.PushNotifications.HasValue)
                    settings.PushNotifications = request.PushNotifications.Value;

                settings.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            return Results.NoContent();
        }

        private static async Task<IResult> GetUserActivities(
            string userId,
            ApplicationDbContext context,
            int page = 1,
            int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;

            var activities = await context.UserActivities
                .Where(ua => ua.UserId == userId)
                .Include(ua => ua.User)
                .OrderByDescending(ua => ua.Timestamp)
                .Skip(skip)
                .Take(pageSize)
                .Select(ua => new UserActivityModel
                {
                    Id = ua.Id,
                    UserId = ua.UserId,
                    UserName = ua.User.UserName ?? string.Empty,
                    Type = ua.Type.ToString(),
                    Description = ua.Description,
                    Timestamp = ua.Timestamp,
                    IpAddress = ua.IpAddress
                })
                .ToListAsync();

            var totalCount = await context.UserActivities
                .CountAsync(ua => ua.UserId == userId);

            var result = new
            {
                Data = activities,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Results.Ok(result);
        }

        private static async Task<IResult> LogUserActivity(
            UserActivityModel request,
            ApplicationDbContext context,
            HttpContext httpContext)
        {
            var activity = new UserActivity
            {
                UserId = request.UserId,
                Type = Enum.Parse<ActivityType>(request.Type),
                Description = request.Description,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers.UserAgent.ToString()
            };

            context.UserActivities.Add(activity);
            await context.SaveChangesAsync();

            return Results.Created($"/api/users/activities/{activity.Id}", new { id = activity.Id });
        }
    }
}
