using LMS.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<AssessmentAttempt> AssessmentAttempts { get; set; }
        public DbSet<QuestionResponse> QuestionResponses { get; set; }
        public DbSet<ModuleProgress> ModuleProgresses { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<ForumTopic> ForumTopics { get; set; }
        public DbSet<ForumPost> ForumPosts { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageAttachment> MessageAttachments { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<CourseTags> CourseTags { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }
        public DbSet<LeaderboardEntry> LeaderboardEntries { get; set; }
        public DbSet<LessonResource> LessonResources { get; set; }
        public DbSet<Certificate> Certificates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure composite keys
            builder.Entity<CourseCategory>()
                .HasKey(cc => new { cc.CourseId, cc.CategoryId });

            builder.Entity<CourseTags>()
                .HasKey(ct => new { ct.CourseId, ct.TagId });

            // Configure relationships
            builder.Entity<Message>()
                .HasOne(m => m.FromUser)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.ToUser)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes for better performance
            builder.Entity<Course>()
                .HasIndex(c => c.Status);

            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.UserId, e.CourseId })
                .IsUnique();

            builder.Entity<AssessmentAttempt>()
                .HasIndex(aa => new { aa.EnrollmentId, aa.AssessmentId, aa.AttemptNumber });

            // Seed default data
            //SeedData(builder);
        }

        //private void SeedData(ModelBuilder builder)
        //{
        //    // Seed default categories
        //    builder.Entity<Category>().HasData(
        //        new Category { Id = 1, Name = "Technology", Description = "Technology and Programming courses", Color = "#007bff" },
        //        new Category { Id = 2, Name = "Business", Description = "Business and Management courses", Color = "#28a745" },
        //        new Category { Id = 3, Name = "Design", Description = "Design and Creative courses", Color = "#dc3545" },
        //        new Category { Id = 4, Name = "Science", Description = "Science and Research courses", Color = "#6f42c1" }
        //    );

        //    // Seed default achievements
        //    builder.Entity<Achievement>().HasData(
        //        new Achievement { Id = 1, Name = "First Steps", Description = "Complete your first lesson", Points = 10, Type = AchievementType.Course },
        //        new Achievement { Id = 2, Name = "Course Completion", Description = "Complete your first course", Points = 100, Type = AchievementType.Course },
        //        new Achievement { Id = 3, Name = "Perfect Score", Description = "Get 100% on an assessment", Points = 50, Type = AchievementType.Assessment },
        //        new Achievement { Id = 4, Name = "Active Learner", Description = "Complete 5 courses", Points = 500, Type = AchievementType.Course }
        //    );
        //}
    }
}
