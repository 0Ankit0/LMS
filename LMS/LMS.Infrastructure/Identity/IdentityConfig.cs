using LMS.Data.Entities.Entities;
using LMS.Infrastructure;
using LMS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LMS.Config
{
    public static class IdentityConfig
    {
        public static void AddIdentityAndDb(this IServiceCollection services, IConfiguration configuration)
        {
            // Get connection string with better error handling
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection string is not configured. Please check your appsettings.json file.");
            }

            //services.AddDbContext<AuthDbContext>(options =>
            //{
            //    options.UseNpgsql(connectionString);
            //    options.EnableSensitiveDataLogging(true); // Enable for development
            //    options.EnableDetailedErrors(true); // Enable for development
            //    options.LogTo(Console.WriteLine, LogLevel.Information);
            //});

            services.AddDbContextFactory<AuthDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.EnableSensitiveDataLogging(true);
                options.EnableDetailedErrors(true);
                options.LogTo(Console.WriteLine, LogLevel.Information);
            });

            services.AddAuthentication()
             .AddBearerToken(IdentityConstants.BearerScheme);

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                // Additional password requirements for educational environment
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // Lockout settings
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
               .AddEntityFrameworkStores<AuthDbContext>()
               .AddDefaultTokenProviders();

            services.AddScoped<IEmailSender<User>, IdentityEmailSender>(); //for identity-specific email sending
            services.AddScoped<IEmailSender, GeneralEmailSender>(); //for general email sending
            services.AddScoped<ISmsSender, SmsSender>();

            // Add authorization policies for LMS roles
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireInstructorRole", policy =>
                    policy.RequireRole("Instructor", "Admin"));
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin"));
                options.AddPolicy("RequireStudentRole", policy =>
                    policy.RequireRole("Student", "Instructor", "Admin"));
            });
        }
    }
}
