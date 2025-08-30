using LMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Web.Data
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
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }

        // New entities for enhanced user management
        public DbSet<UserFile> UserFiles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<ParentStudentLink> ParentStudentLinks { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<AchievementCriteria> AchievementCriteria { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure computed properties to be ignored by EF
            builder.Entity<ModuleProgress>()
                .Ignore(mp => mp.IsCompleted);

            builder.Entity<LessonProgress>()
                .Ignore(lp => lp.IsCompleted);

            // Configure composite keys
            builder.Entity<CourseCategory>()
                .HasKey(cc => new { cc.CourseId, cc.CategoryId });

            builder.Entity<CourseTags>()
                .HasKey(ct => new { ct.CourseId, ct.TagId });

            // Configure Attendance relationships
            builder.Entity<Attendance>()
                .HasOne(a => a.Class)
                .WithMany()
                .HasForeignKey(a => a.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attendance>()
                .HasOne(a => a.Instructor)
                .WithMany()
                .HasForeignKey(a => a.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure only one of StudentId or InstructorId is set
            builder.Entity<Attendance>()
                .ToTable(t => t.HasCheckConstraint("CK_Attendance_OneUserType",
                    "(\"StudentId\" IS NOT NULL AND \"InstructorId\" IS NULL) OR (\"StudentId\" IS NULL AND \"InstructorId\" IS NOT NULL)"));

            // Configure Course relationships
            builder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany()
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Enrollment relationships
            builder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Module relationships
            builder.Entity<Module>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Lesson relationships
            builder.Entity<Lesson>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Assessment relationships
            builder.Entity<Assessment>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Assessments)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Assessment>()
                .HasOne(a => a.Module)
                .WithMany()
                .HasForeignKey(a => a.ModuleId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Question relationships
            builder.Entity<Question>()
                .HasOne(q => q.Assessment)
                .WithMany(a => a.Questions)
                .HasForeignKey(q => q.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure QuestionOption relationships
            builder.Entity<QuestionOption>()
                .HasOne(qo => qo.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(qo => qo.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure AssessmentAttempt relationships
            builder.Entity<AssessmentAttempt>()
                .HasOne(aa => aa.Assessment)
                .WithMany(a => a.Attempts)
                .HasForeignKey(aa => aa.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AssessmentAttempt>()
                .HasOne(aa => aa.Enrollment)
                .WithMany(e => e.AssessmentAttempts)
                .HasForeignKey(aa => aa.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Progress relationships
            builder.Entity<ModuleProgress>()
                .HasOne(mp => mp.Enrollment)
                .WithMany(e => e.ModuleProgresses)
                .HasForeignKey(mp => mp.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ModuleProgress>()
                .HasOne(mp => mp.Module)
                .WithMany(m => m.ModuleProgresses)
                .HasForeignKey(mp => mp.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LessonProgress>()
                .HasOne(lp => lp.ModuleProgress)
                .WithMany(mp => mp.LessonProgresses)
                .HasForeignKey(lp => lp.ModuleProgressId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LessonProgress>()
                .HasOne(lp => lp.Lesson)
                .WithMany(l => l.LessonProgresses)
                .HasForeignKey(lp => lp.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Forum relationships
            builder.Entity<Forum>()
                .HasOne(f => f.Course)
                .WithMany(c => c.Forums)
                .HasForeignKey(f => f.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ForumTopic>()
                .HasOne(ft => ft.Forum)
                .WithMany(f => f.Topics)
                .HasForeignKey(ft => ft.ForumId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ForumPost>()
                .HasOne(fp => fp.Topic)
                .WithMany(ft => ft.Posts)
                .HasForeignKey(fp => fp.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ForumPost>()
                .HasOne(fp => fp.Author)
                .WithMany(u => u.ForumPosts)
                .HasForeignKey(fp => fp.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Category relationships
            builder.Entity<CourseCategory>()
                .HasOne(cc => cc.Course)
                .WithMany(c => c.CourseCategories)
                .HasForeignKey(cc => cc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseCategory>()
                .HasOne(cc => cc.Category)
                .WithMany(c => c.CourseCategories)
                .HasForeignKey(cc => cc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Tag relationships
            builder.Entity<CourseTags>()
                .HasOne(ct => ct.Course)
                .WithMany(c => c.CourseTags)
                .HasForeignKey(ct => ct.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseTags>()
                .HasOne(ct => ct.Tag)
                .WithMany(t => t.CourseTags)
                .HasForeignKey(ct => ct.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Achievement relationships
            builder.Entity<UserAchievement>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Achievements)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserAchievement>()
                .HasOne(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Leaderboard relationships
            builder.Entity<LeaderboardEntry>()
                .HasOne(le => le.Leaderboard)
                .WithMany(l => l.Entries)
                .HasForeignKey(le => le.LeaderboardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LeaderboardEntry>()
                .HasOne(le => le.User)
                .WithMany()
                .HasForeignKey(le => le.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Certificate relationships
            builder.Entity<Certificate>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Certificate>()
                .HasOne(c => c.Course)
                .WithMany()
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Note relationships
            builder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Note>()
                .HasOne(n => n.Course)
                .WithMany()
                .HasForeignKey(n => n.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Note>()
                .HasOne(n => n.Lesson)
                .WithMany()
                .HasForeignKey(n => n.LessonId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Note indexes
            builder.Entity<Note>()
                .HasIndex(n => new { n.UserId, n.CreatedAt })
                .HasDatabaseName("IX_Note_UserId_CreatedAt");

            builder.Entity<Note>()
                .HasIndex(n => new { n.UserId, n.IsPinned })
                .HasDatabaseName("IX_Note_UserId_IsPinned");

            builder.Entity<Note>()
                .HasIndex(n => new { n.UserId, n.IsDeleted })
                .HasDatabaseName("IX_Note_UserId_IsDeleted");

            builder.Entity<Note>()
                .HasIndex(n => n.Type)
                .HasDatabaseName("IX_Note_Type");

            builder.Entity<Note>()
                .HasIndex(n => n.Priority)
                .HasDatabaseName("IX_Note_Priority");

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

            // Attendance indexes
            builder.Entity<Attendance>()
                .HasIndex(a => new { a.ClassId, a.Date })
                .HasDatabaseName("IX_Attendance_ClassId_Date");

            builder.Entity<Attendance>()
                .HasIndex(a => new { a.StudentId, a.Date })
                .HasDatabaseName("IX_Attendance_StudentId_Date");

            builder.Entity<Attendance>()
                .HasIndex(a => new { a.InstructorId, a.Date })
                .HasDatabaseName("IX_Attendance_InstructorId_Date");

            builder.Entity<Attendance>()
                .HasIndex(a => new { a.Status, a.Date })
                .HasDatabaseName("IX_Attendance_Status_Date");

            builder.Entity<Attendance>()
                .HasIndex(a => a.Date)
                .HasDatabaseName("IX_Attendance_Date");

            // Course indexes
            builder.Entity<Course>()
                .HasIndex(c => c.Status)
                .HasDatabaseName("IX_Course_Status");

            builder.Entity<Course>()
                .HasIndex(c => c.InstructorId)
                .HasDatabaseName("IX_Course_InstructorId");

            builder.Entity<Course>()
                .HasIndex(c => new { c.Status, c.StartDate })
                .HasDatabaseName("IX_Course_Status_StartDate");

            builder.Entity<Course>()
                .HasIndex(c => c.CreatedAt)
                .HasDatabaseName("IX_Course_CreatedAt");

            // Enrollment indexes
            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.UserId, e.CourseId })
                .IsUnique()
                .HasDatabaseName("IX_Enrollment_UserId_CourseId");

            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.Status, e.EnrolledAt })
                .HasDatabaseName("IX_Enrollment_Status_EnrolledAt");

            builder.Entity<Enrollment>()
                .HasIndex(e => e.ProgressPercentage)
                .HasDatabaseName("IX_Enrollment_ProgressPercentage");

            // Assessment indexes
            builder.Entity<Assessment>()
                .HasIndex(a => new { a.CourseId, a.IsActive })
                .HasDatabaseName("IX_Assessment_CourseId_IsActive");

            builder.Entity<Assessment>()
                .HasIndex(a => new { a.Type, a.IsActive })
                .HasDatabaseName("IX_Assessment_Type_IsActive");

            // AssessmentAttempt indexes
            builder.Entity<AssessmentAttempt>()
                .HasIndex(aa => new { aa.EnrollmentId, aa.AssessmentId, aa.AttemptNumber })
                .HasDatabaseName("IX_AssessmentAttempt_EnrollmentId_AssessmentId_AttemptNumber");

            builder.Entity<AssessmentAttempt>()
                .HasIndex(aa => new { aa.Status, aa.CompletedAt })
                .HasDatabaseName("IX_AssessmentAttempt_Status_CompletedAt");

            builder.Entity<AssessmentAttempt>()
                .HasIndex(aa => aa.Score)
                .HasDatabaseName("IX_AssessmentAttempt_Score");

            // Module and Lesson indexes
            builder.Entity<Module>()
                .HasIndex(m => m.CourseId)
                .HasDatabaseName("IX_Module_CourseId");

            builder.Entity<Lesson>()
                .HasIndex(l => l.ModuleId)
                .HasDatabaseName("IX_Lesson_ModuleId");

            // Progress indexes
            builder.Entity<ModuleProgress>()
                .HasIndex(mp => new { mp.EnrollmentId, mp.ModuleId })
                .IsUnique()
                .HasDatabaseName("IX_ModuleProgress_EnrollmentId_ModuleId");

            builder.Entity<ModuleProgress>()
                .HasIndex(mp => mp.CompletedAt)
                .HasDatabaseName("IX_ModuleProgress_CompletedAt");

            builder.Entity<LessonProgress>()
                .HasIndex(lp => new { lp.ModuleProgressId, lp.LessonId })
                .IsUnique()
                .HasDatabaseName("IX_LessonProgress_ModuleProgressId_LessonId");

            // Forum indexes
            builder.Entity<Forum>()
                .HasIndex(f => new { f.CourseId, f.IsActive })
                .HasDatabaseName("IX_Forum_CourseId_IsActive");

            builder.Entity<ForumTopic>()
                .HasIndex(ft => new { ft.ForumId, ft.CreatedAt })
                .HasDatabaseName("IX_ForumTopic_ForumId_CreatedAt");

            builder.Entity<ForumPost>()
                .HasIndex(fp => new { fp.TopicId, fp.CreatedAt })
                .HasDatabaseName("IX_ForumPost_TopicId_CreatedAt");

            builder.Entity<ForumPost>()
                .HasIndex(fp => fp.AuthorId)
                .HasDatabaseName("IX_ForumPost_AuthorId");

            // Message indexes
            builder.Entity<Message>()
                .HasIndex(m => new { m.ToUserId, m.SentAt })
                .HasDatabaseName("IX_Message_ToUserId_SentAt");

            builder.Entity<Message>()
                .HasIndex(m => new { m.FromUserId, m.SentAt })
                .HasDatabaseName("IX_Message_FromUserId_SentAt");

            // User-related indexes
            builder.Entity<User>()
                .HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_User_IsActive");

            builder.Entity<User>()
                .HasIndex(u => new { u.TotalPoints, u.Level })
                .HasDatabaseName("IX_User_TotalPoints_Level");

            builder.Entity<User>()
                .HasIndex(u => u.LastLoginAt)
                .HasDatabaseName("IX_User_LastLoginAt");

            // Certificate indexes
            builder.Entity<Certificate>()
                .HasIndex(c => new { c.UserId, c.IssuedAt })
                .HasDatabaseName("IX_Certificate_UserId_IssuedAt");

            builder.Entity<Certificate>()
                .HasIndex(c => new { c.CourseId, c.IssuedAt })
                .HasDatabaseName("IX_Certificate_CourseId_IssuedAt");

            // Achievement indexes
            builder.Entity<UserAchievement>()
                .HasIndex(ua => new { ua.UserId, ua.EarnedAt })
                .HasDatabaseName("IX_UserAchievement_UserId_EarnedAt");

            // Leaderboard indexes
            builder.Entity<LeaderboardEntry>()
                .HasIndex(le => new { le.LeaderboardId, le.Score })
                .HasDatabaseName("IX_LeaderboardEntry_LeaderboardId_Score");

            // Announcement indexes
            builder.Entity<Announcement>()
                .HasIndex(a => a.IsActive)
                .HasDatabaseName("IX_Announcement_IsActive");

            // Configure new entity relationships for enhanced user management

            // UserFile relationships
            builder.Entity<UserFile>()
                .HasOne(uf => uf.Owner)
                .WithMany(u => u.UploadedFiles)
                .HasForeignKey(uf => uf.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<User>()
                .HasOne(u => u.ProfilePictureFile)
                .WithMany(uf => uf.UserProfilePictures)
                .HasForeignKey(u => u.ProfilePictureFileId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Course>()
                .HasOne(c => c.ThumbnailFile)
                .WithMany(uf => uf.CourseThumbnails)
                .HasForeignKey(c => c.ThumbnailFileId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Category>()
                .HasOne(c => c.IconFile)
                .WithMany(uf => uf.CategoryIcons)
                .HasForeignKey(c => c.IconFileId)
                .OnDelete(DeleteBehavior.SetNull);

            // Extended user profile relationships
            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.StudentProfile)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Instructor>()
                .HasOne(i => i.User)
                .WithOne(u => u.InstructorProfile)
                .HasForeignKey<Instructor>(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Parent>()
                .HasOne(p => p.User)
                .WithOne(u => u.ParentProfile)
                .HasForeignKey<Parent>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ParentStudentLink composite key and relationships
            builder.Entity<ParentStudentLink>()
                .HasKey(psl => new { psl.ParentId, psl.StudentId });

            builder.Entity<ParentStudentLink>()
                .HasOne(psl => psl.Parent)
                .WithMany(p => p.StudentLinks)
                .HasForeignKey(psl => psl.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ParentStudentLink>()
                .HasOne(psl => psl.Student)
                .WithMany(s => s.ParentLinks)
                .HasForeignKey(psl => psl.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserSettings relationship
            builder.Entity<UserSettings>()
                .HasOne(us => us.User)
                .WithOne(u => u.Settings)
                .HasForeignKey<UserSettings>(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserActivity relationship
            builder.Entity<UserActivity>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AchievementCriteria relationship
            builder.Entity<AchievementCriteria>()
                .HasOne(ac => ac.Achievement)
                .WithMany(a => a.Criteria)
                .HasForeignKey(ac => ac.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for new entities
            builder.Entity<UserFile>()
                .HasIndex(uf => uf.OwnerUserId)
                .HasDatabaseName("IX_UserFile_OwnerUserId");

            builder.Entity<UserFile>()
                .HasIndex(uf => new { uf.ContentType, uf.IsActive })
                .HasDatabaseName("IX_UserFile_ContentType_IsActive");

            builder.Entity<Student>()
                .HasIndex(s => s.StudentIdNumber)
                .IsUnique()
                .HasFilter("[StudentIdNumber] IS NOT NULL")
                .HasDatabaseName("IX_Student_StudentIdNumber");

            builder.Entity<UserActivity>()
                .HasIndex(ua => new { ua.UserId, ua.Timestamp })
                .HasDatabaseName("IX_UserActivity_UserId_Timestamp");

            builder.Entity<UserActivity>()
                .HasIndex(ua => new { ua.Type, ua.Timestamp })
                .HasDatabaseName("IX_UserActivity_Type_Timestamp");

            // Seed default data
            //LMS.Data.DataSeeder.Seed(builder);
            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            // Seed roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "Instructor",
                    NormalizedName = "INSTRUCTOR"
                },
                new IdentityRole
                {
                    Id = "3",
                    Name = "Student",
                    NormalizedName = "STUDENT"
                }
            );

            var userId = "11111111-1111-1111-1111-111111111111";

            // Seed admin user with static values only
            var adminUser = new User
            {
                Id = userId,
                UserName = "admin",
                NormalizedUserName = "ADMIN@LMS.COM",
                Email = "admin@lms.com",
                NormalizedEmail = "ADMIN@LMS.COM",
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                SecurityStamp = "static-security-stamp",
                ConcurrencyStamp = "static-concurrency-stamp",
                PasswordHash = "AQAAAAIAAYagAAAAEA1temm06r92NHot4EpXWUdFi1zrgQeD0XRckNiuy+FlGRidLjTFDUqVsm4GmoiKIQ==", // Pre-hashed Admin@123
                Level = 1,
                TotalPoints = 0,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PhoneNumberConfirmed = false
            };

            builder.Entity<User>().HasData(adminUser);

            // Assign admin role to admin user
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = userId,
                    RoleId = "1"
                }
            );
        }
    }
}