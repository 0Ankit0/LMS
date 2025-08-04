
using LMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace LMS.Data
{
    public static class DataSeeder
    {
        public static void Seed(ModelBuilder builder)
        {
            var hasher = new PasswordHasher<User>();

            var users = new User[]
            {
                new User { Id = "1", UserName = "teacher@lms.com", NormalizedUserName = "TEACHER@LMS.COM", Email = "teacher@lms.com", NormalizedEmail = "TEACHER@LMS.COM", EmailConfirmed = true, PasswordHash = hasher.HashPassword(new User(), "Password123!") },
                new User { Id = "2", UserName = "student1@lms.com", NormalizedUserName = "STUDENT1@LMS.COM", Email = "student1@lms.com", NormalizedEmail = "STUDENT1@LMS.COM", EmailConfirmed = true, PasswordHash = hasher.HashPassword(new User(), "Password123!") },
                new User { Id = "3", UserName = "student2@lms.com", NormalizedUserName = "STUDENT2@LMS.COM", Email = "student2@lms.com", NormalizedEmail = "STUDENT2@LMS.COM", EmailConfirmed = true, PasswordHash = hasher.HashPassword(new User(), "Password123!") },
            };

            builder.Entity<User>().HasData(users);

            var courses = new Course[]
            {
                new Course { Id = 1, Title = "Introduction to C#", Description = "Learn the fundamentals of C# and .NET.", InstructorId = "1" },
                new Course { Id = 2, Title = "Web Development with Blazor", Description = "Build interactive web UIs with C# and Blazor.", InstructorId = "1" },
            };

            builder.Entity<Course>().HasData(courses);

            var enrollments = new Enrollment[]
            {
                new Enrollment { Id = 1, CourseId = 1, UserId = "2" },
                new Enrollment { Id = 2, CourseId = 1, UserId = "3" },
                new Enrollment { Id = 3, CourseId = 2, UserId = "2" },
            };

            builder.Entity<Enrollment>().HasData(enrollments);
        }
    }
}
