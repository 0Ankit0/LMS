using Microsoft.EntityFrameworkCore;
using LMS.Data.Entities;
using System;

namespace LMS.Web.Data
{
    public static class DataSeeder
    {
        public static void Seed(ModelBuilder builder)
        {
            // Seed Users
            builder.Entity<User>().HasData(new User
            {
                Id = "seed-user-1",
                UserName = "jdoe",
                NormalizedUserName = "JDOE",
                Email = "jdoe@example.com",
                NormalizedEmail = "JDOE@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = "seed-stamp",
                ConcurrencyStamp = "seed-concurrency",
                PasswordHash = "",
                Level = 1,
                TotalPoints = 100,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PhoneNumberConfirmed = false
            });

            // Seed Categories
            builder.Entity<Category>().HasData(new Category
            {
                Id = 1,
                Name = "Programming",
                Description = "Programming courses",
                Color = "#007bff",
                IsActive = true
            });

            // Seed Tags
            builder.Entity<Tag>().HasData(new Tag
            {
                Id = 1,
                Name = "C#",
                Color = "#6c757d"
            });

            // Seed Courses
            builder.Entity<Course>().HasData(new Course
            {
                Id = 1,
                Title = "C# Basics",
                Description = "Learn the basics of C#.",
                InstructorId = "seed-user-1",
                Level = CourseLevel.Beginner,
                Status = CourseStatus.Published,
                MaxEnrollments = 100,
                StartDate = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            // Seed CourseCategory
            builder.Entity<CourseCategory>().HasData(new CourseCategory
            {
                CourseId = 1,
                CategoryId = 1
            });

            // Seed CourseTags
            builder.Entity<CourseTags>().HasData(new CourseTags
            {
                CourseId = 1,
                TagId = 1
            });

            // Seed Module
            builder.Entity<Module>().HasData(new Module
            {
                Id = 1,
                Title = "Introduction",
                Description = "Intro to C#",
                CourseId = 1,
                OrderIndex = 1,
                IsRequired = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            // Seed Lesson
            builder.Entity<Lesson>().HasData(new Lesson
            {
                Id = 1,
                Title = "What is C#?",
                ModuleId = 1,
                Type = LessonType.Text,
                OrderIndex = 1,
                IsRequired = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            // Seed Enrollment
            builder.Entity<Enrollment>().HasData(new Enrollment
            {
                Id = 1,
                UserId = "seed-user-1",
                CourseId = 1,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Active,
                ProgressPercentage = 0
            });

            // Seed Assessment
            builder.Entity<Assessment>().HasData(new Assessment
            {
                Id = 1,
                Title = "Quiz 1",
                CourseId = 1,
                Type = AssessmentType.Quiz,
                MaxAttempts = 3,
                PassingScore = 70,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            // Seed Question
            builder.Entity<Question>().HasData(new Question
            {
                Id = 1,
                Text = "What is C#?",
                AssessmentId = 1,
                Type = QuestionType.MultipleChoice,
                Points = 1,
                OrderIndex = 1,
                IsRequired = true,
                CreatedAt = DateTime.UtcNow
            });

            // Seed QuestionOption
            builder.Entity<QuestionOption>().HasData(new QuestionOption
            {
                Id = 1,
                Text = "A programming language",
                QuestionId = 1,
                IsCorrect = true,
                OrderIndex = 1
            });

            // Seed Forum
            builder.Entity<Forum>().HasData(new Forum
            {
                Id = 1,
                Title = "General Discussion",
                CourseId = 1,
                IsGeneral = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // Seed ForumTopic
            builder.Entity<ForumTopic>().HasData(new ForumTopic
            {
                Id = 1,
                Title = "Welcome!",
                ForumId = 1,
                CreatedByUserId = "seed-user-1",
                IsPinned = true,
                IsLocked = false,
                CreatedAt = DateTime.UtcNow
            });

            // Seed ForumPost
            builder.Entity<ForumPost>().HasData(new ForumPost
            {
                Id = 1,
                Content = "Hello everyone!",
                TopicId = 1,
                AuthorId = "seed-user-1",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            // Seed Achievement
            builder.Entity<Achievement>().HasData(new Achievement
            {
                Id = 1,
                Name = "First Course Completed",
                Description = "Complete your first course.",
                Points = 50,
                BadgeColor = "#ffd700",
                Type = AchievementType.Course,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // Seed UserAchievement
            builder.Entity<UserAchievement>().HasData(new UserAchievement
            {
                Id = 1,
                UserId = "seed-user-1",
                AchievementId = 1,
                EarnedAt = DateTime.UtcNow
            });

            // Seed Leaderboard
            builder.Entity<Leaderboard>().HasData(new Leaderboard
            {
                Id = 1,
                Name = "Top Students",
                Type = LeaderboardType.Points,
                Period = LeaderboardPeriod.AllTime,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // Seed LeaderboardEntry
            builder.Entity<LeaderboardEntry>().HasData(new LeaderboardEntry
            {
                Id = 1,
                LeaderboardId = 1,
                UserId = "seed-user-1",
                Rank = 1,
                Score = 100,
                LastUpdated = DateTime.UtcNow
            });

            // Seed Certificate
            builder.Entity<Certificate>().HasData(new Certificate
            {
                Id = 1,
                UserId = "seed-user-1",
                CourseId = 1,
                CertificateNumber = "CERT-001",
                IssuedAt = DateTime.UtcNow,
                FinalGrade = 95,
                IsValid = true
            });

            // Seed Attendance
            builder.Entity<Attendance>().HasData(new Attendance
            {
                Id = 1,
                ClassId = 1,
                StudentId = "seed-user-1",
                Date = DateTime.UtcNow,
                Status = AttendanceStatus.Present,
                IsActive = true
            });

            // Seed Communication (Message)
            builder.Entity<Message>().HasData(new Message
            {
                Id = 1,
                Subject = "Welcome",
                Content = "Welcome to the LMS!",
                FromUserId = "seed-user-1",
                ToUserId = "seed-user-1",
                SentAt = DateTime.UtcNow,
                Priority = MessagePriority.Normal
            });

            // Seed Announcement
            builder.Entity<Announcement>().HasData(new Announcement
            {
                Id = 1,
                Title = "System Launch",
                Content = "The LMS is now live!",
                AuthorId = "seed-user-1",
                CourseId = 1,
                Type = AnnouncementType.General,
                Priority = AnnouncementPriority.Normal,
                PublishedAt = DateTime.UtcNow,
                IsActive = true
            });
        }
    }
}
