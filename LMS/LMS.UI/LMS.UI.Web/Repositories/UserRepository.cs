using LMS.Data.DTOs;
using LMS.Data.Entities;

using LMS.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LMS.Repositories
{
    public interface IUserRepository
    {
        Task<List<UserModel>> GetUsersAsync();
        Task<UserModel?> GetUserByIdAsync(string id);
        Task<UserModel> CreateUserAsync(UpdateUserProfileRequest request);
        Task<UserModel> UpdateUserAsync(string id, UpdateUserProfileRequest request);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> ToggleUserStatusAsync(string id);
        Task<List<EnrollmentModel>> GetUserEnrollmentsAsync(string userId);
        Task<List<UserAchievementModel>> GetUserAchievementsAsync(string userId);
        Task<EnrollmentModel> CreateEnrollmentAsync(string userId, CreateEnrollmentRequest request);
        Task<bool> UpdateProgressAsync(string userId, UpdateProgressRequest request);
    }

    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ApplicationDbContext context, UserManager<User> userManager, ILogger<UserRepository> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<List<UserModel>> GetUsersAsync()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .Include(u => u.Achievements)
                        .ThenInclude(ua => ua.Achievement)
                    .ToListAsync();

                var userModels = new List<UserModel>();
                foreach (var user in users)
                {
                    var userModel = await MapToUserModelAsync(user);
                    userModels.Add(userModel);
                }

                return userModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                throw;
            }
        }

        public async Task<UserModel?> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Enrollments)
                        .ThenInclude(e => e.Course)
                    .Include(u => u.Achievements)
                        .ThenInclude(ua => ua.Achievement)
                    .FirstOrDefaultAsync(u => u.Id == id);

                return user != null ? await MapToUserModelAsync(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching user by id {id}");
                throw;
            }
        }

        public async Task<UserModel> CreateUserAsync(UpdateUserProfileRequest request)
        {
            try
            {
                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Bio = request.Bio,
                    ProfilePictureUrl = request.ProfilePictureUrl,
                    DateOfBirth = request.DateOfBirth,
                    PhoneNumber = request.PhoneNumber,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "TempPassword123!");
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                // Add role
                if (!string.IsNullOrEmpty(request.Role))
                {
                    await _userManager.AddToRoleAsync(user, request.Role);
                }

                return await MapToUserModelAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public async Task<UserModel> UpdateUserAsync(string id, UpdateUserProfileRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    throw new ArgumentException("User not found");

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;
                user.PhoneNumber = request.PhoneNumber;
                user.Bio = request.Bio;
                user.ProfilePictureUrl = request.ProfilePictureUrl;
                user.DateOfBirth = request.DateOfBirth;

                await _context.SaveChangesAsync();

                // Update role if changed
                if (!string.IsNullOrEmpty(request.Role))
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, request.Role);
                }

                return await MapToUserModelAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user {id}");
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return false;

                user.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user {id}");
                throw;
            }
        }

        public async Task<bool> ToggleUserStatusAsync(string id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return false;

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling user status {id}");
                throw;
            }
        }

        public async Task<List<EnrollmentModel>> GetUserEnrollmentsAsync(string userId)
        {
            try
            {
                var enrollments = await _context.Enrollments
                    .Where(e => e.UserId == userId)
                    .Include(e => e.Course)
                    .Include(e => e.ModuleProgresses)
                        .ThenInclude(mp => mp.Module)
                    .OrderByDescending(e => e.EnrolledAt)
                    .ToListAsync();

                return enrollments.Select(MapToEnrollmentModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching enrollments for user {userId}");
                throw;
            }
        }

        public async Task<List<UserAchievementModel>> GetUserAchievementsAsync(string userId)
        {
            try
            {
                var achievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .Include(ua => ua.Achievement)
                    .OrderByDescending(ua => ua.EarnedAt)
                    .ToListAsync();

                return achievements.Select(MapToUserAchievementModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching achievements for user {userId}");
                throw;
            }
        }

        public async Task<EnrollmentModel> CreateEnrollmentAsync(string userId, CreateEnrollmentRequest request)
        {
            try
            {
                // Check if user is already enrolled
                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == request.CourseId);

                if (existingEnrollment != null)
                    throw new InvalidOperationException("User is already enrolled in this course");

                // Check course capacity
                var course = await _context.Courses
                    .Include(c => c.Enrollments)
                    .FirstOrDefaultAsync(c => c.Id == request.CourseId);

                if (course == null)
                    throw new ArgumentException("Course not found");

                if (course.MaxEnrollments > 0 && course.Enrollments.Count >= course.MaxEnrollments)
                    throw new InvalidOperationException("Course is full");

                var enrollment = new Enrollment
                {
                    UserId = userId,
                    CourseId = request.CourseId,
                    EnrolledAt = DateTime.UtcNow,
                    Status = EnrollmentStatus.Active,
                    ProgressPercentage = 0,
                    TimeSpent = TimeSpan.Zero
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                var createdEnrollment = await _context.Enrollments
                    .Include(e => e.Course)
                    .Include(e => e.ModuleProgresses)
                    .FirstOrDefaultAsync(e => e.Id == enrollment.Id);

                return MapToEnrollmentModel(createdEnrollment!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating enrollment for user {userId}");
                throw;
            }
        }

        public async Task<bool> UpdateProgressAsync(string userId, UpdateProgressRequest request)
        {
            try
            {
                // Find the enrollment first
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == request.EnrollmentId);

                if (enrollment == null)
                    return false;

                // Update lesson progress if LessonId is provided
                if (request.LessonId.HasValue)
                {
                    // Find or create module progress first
                    var moduleProgress = await _context.ModuleProgresses
                        .FirstOrDefaultAsync(mp => mp.EnrollmentId == request.EnrollmentId && mp.ModuleId == request.ModuleId);

                    if (moduleProgress == null && request.ModuleId.HasValue)
                    {
                        moduleProgress = new ModuleProgress
                        {
                            EnrollmentId = request.EnrollmentId,
                            ModuleId = request.ModuleId.Value,
                            StartedAt = DateTime.UtcNow,
                            ProgressPercentage = 0,
                            TimeSpent = TimeSpan.Zero
                        };
                        _context.ModuleProgresses.Add(moduleProgress);
                        await _context.SaveChangesAsync();
                    }

                    if (moduleProgress != null)
                    {
                        // Find or create lesson progress
                        var lessonProgress = await _context.LessonProgresses
                            .FirstOrDefaultAsync(lp => lp.ModuleProgressId == moduleProgress.Id && lp.LessonId == request.LessonId);

                        if (lessonProgress == null)
                        {
                            lessonProgress = new LessonProgress
                            {
                                ModuleProgressId = moduleProgress.Id,
                                LessonId = request.LessonId.Value,
                                StartedAt = DateTime.UtcNow,
                                TimeSpent = request.TimeSpent,
                                ProgressPercentage = request.ProgressPercentage
                            };

                            if (request.IsCompleted)
                            {
                                lessonProgress.CompletedAt = DateTime.UtcNow;
                            }

                            _context.LessonProgresses.Add(lessonProgress);
                        }
                        else
                        {
                            lessonProgress.TimeSpent = request.TimeSpent;
                            lessonProgress.ProgressPercentage = request.ProgressPercentage;

                            if (request.IsCompleted && !lessonProgress.CompletedAt.HasValue)
                            {
                                lessonProgress.CompletedAt = DateTime.UtcNow;
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating progress for user {userId}");
                throw;
            }
        }

        private async Task<UserModel> MapToUserModelAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new UserModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                DateOfBirth = user.DateOfBirth,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                TotalPoints = user.TotalPoints,
                Level = user.Level,
                Roles = roles.ToList(),
                Enrollments = user.Enrollments?.Select(MapToEnrollmentModel).ToList() ?? new List<EnrollmentModel>(),
                Achievements = user.Achievements?.Select(MapToUserAchievementModel).ToList() ?? new List<UserAchievementModel>()
            };
        }

        private static EnrollmentModel MapToEnrollmentModel(Enrollment enrollment)
        {
            return new EnrollmentModel
            {
                Id = enrollment.Id,
                UserId = enrollment.UserId,
                UserName = enrollment.User?.UserName ?? string.Empty,
                CourseId = enrollment.CourseId,
                CourseTitle = enrollment.Course?.Title ?? string.Empty,
                CourseThumbnailUrl = enrollment.Course?.ThumbnailUrl ?? string.Empty,
                EnrolledAt = enrollment.EnrolledAt,
                StartedAt = enrollment.StartedAt,
                CompletedAt = enrollment.CompletedAt,
                Status = enrollment.Status.ToString(),
                ProgressPercentage = enrollment.ProgressPercentage,
                TimeSpent = enrollment.TimeSpent,
                FinalGrade = enrollment.FinalGrade,
                IsCertificateIssued = enrollment.IsCertificateIssued,
                CertificateIssuedAt = enrollment.CertificateIssuedAt,
                ModuleProgresses = enrollment.ModuleProgresses?.Select(mp => new ModuleProgressModel
                {
                    Id = mp.Id,
                    ModuleId = mp.ModuleId,
                    ModuleTitle = mp.Module?.Title ?? string.Empty,
                    StartedAt = mp.StartedAt,
                    CompletedAt = mp.CompletedAt,
                    ProgressPercentage = mp.ProgressPercentage,
                    TimeSpent = mp.TimeSpent,
                    IsCompleted = mp.IsCompleted
                }).ToList() ?? new List<ModuleProgressModel>()
            };
        }

        private static UserAchievementModel MapToUserAchievementModel(UserAchievement userAchievement)
        {
            return new UserAchievementModel
            {
                Id = userAchievement.Id,
                AchievementName = userAchievement.Achievement.Name,
                AchievementDescription = userAchievement.Achievement.Description,
                AchievementIconUrl = userAchievement.Achievement.IconUrl,
                Points = userAchievement.Achievement.Points,
                BadgeColor = userAchievement.Achievement.BadgeColor,
                EarnedAt = userAchievement.EarnedAt
            };
        }

        private static int CalculateLevel(int totalPoints)
        {
            // Simple level calculation - every 100 points is a level
            return (totalPoints / 100) + 1;
        }

        private static string GetBadgeIcon(string type) => type switch
        {
            "Course" => "fas fa-graduation-cap",
            "Assessment" => "fas fa-star",
            "Participation" => "fas fa-comments",
            "Time" => "fas fa-clock",
            "Streak" => "fas fa-fire",
            "Social" => "fas fa-users",
            _ => "fas fa-award"
        };
    }
}