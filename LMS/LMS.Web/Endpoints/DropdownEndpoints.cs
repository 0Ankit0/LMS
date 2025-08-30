using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Infrastructure;
using LMS.Web.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LMS.Web.Endpoints
{
    public class DropdownEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/dropdown").WithTags("Dropdowns");

            group.MapGet("/categories", GetCategoriesDropdown)
                .WithName("GetCategoriesDropdown")
                .WithSummary("Get categories for use in dropdowns");

            group.MapGet("/course-levels", GetCourseLevelsDropdown)
                .WithName("GetCourseLevelsDropdown")
                .WithSummary("Get available course levels");

            group.MapGet("/users", GetUsersDropdown)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("GetUsersDropdown")
                .WithSummary("Get users for assigning instructors");

            group.MapGet("/instructors", GetInstructorsDropdown)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("GetInstructorsDropdown")
                .WithSummary("Get instructors for course assignment");

            group.MapGet("/courses", GetCoursesDropdown)
                .RequireAuthorization()
                .WithName("GetCoursesDropdown")
                .WithSummary("Get courses for dropdowns");

            group.MapGet("/tags", GetTagsDropdown)
                .WithName("GetTagsDropdown")
                .WithSummary("Get tags for dropdowns");
        }

        private static async Task<IResult> GetCategoriesDropdown(IDashboardRepository dashboardRepository)
        {
            try
            {
                // For now, return a simple list - this should be enhanced later
                var dropdownOptions = new List<DropdownOption>
                {
                    new() { Value = "1", Text = "Programming" },
                    new() { Value = "2", Text = "Design" },
                    new() { Value = "3", Text = "Business" },
                    new() { Value = "4", Text = "Marketing" },
                    new() { Value = "5", Text = "Data Science" }
                };

                return Results.Ok(dropdownOptions);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving categories dropdown: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCourseLevelsDropdown()
        {
            try
            {
                var dropdownOptions = new List<DropdownOption>
                {
                    new() { Value = "Beginner", Text = "Beginner" },
                    new() { Value = "Intermediate", Text = "Intermediate" },
                    new() { Value = "Advanced", Text = "Advanced" },
                    new() { Value = "Expert", Text = "Expert" }
                };

                return Results.Ok(dropdownOptions);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving course levels dropdown: {ex.Message}");
            }
        }

        private static async Task<IResult> GetUsersDropdown(
            IDashboardRepository dashboardRepository,
            ClaimsPrincipal user,
            string? role = null)
        {
            try
            {
                // For now, return a simple list - this should be enhanced later
                var dropdownOptions = new List<DropdownOption>
                {
                    new() { Value = "user1", Text = "John Doe (Instructor)" },
                    new() { Value = "user2", Text = "Jane Smith (Instructor)" },
                    new() { Value = "user3", Text = "Bob Johnson (Admin)" }
                };

                return Results.Ok(dropdownOptions);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving users dropdown: {ex.Message}");
            }
        }

        private static async Task<IResult> GetInstructorsDropdown(IDashboardRepository dashboardRepository)
        {
            try
            {
                // For now, return a simple list - this should be enhanced later
                var dropdownOptions = new List<DropdownOption>
                {
                    new() { Value = "instructor1", Text = "Dr. John Smith" },
                    new() { Value = "instructor2", Text = "Prof. Jane Doe" },
                    new() { Value = "instructor3", Text = "Mr. Robert Brown" }
                };

                return Results.Ok(dropdownOptions);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving instructors dropdown: {ex.Message}");
            }
        }

        private static async Task<IResult> GetCoursesDropdown(
            IDashboardRepository dashboardRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var courses = await dashboardRepository.GetUserCoursesAsync(userId);
                var dropdownOptions = courses.Select(c => new DropdownOption
                {
                    Value = c.Id.ToString(),
                    Text = c.Title
                }).ToList();

                return Results.Ok(dropdownOptions);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving courses dropdown: {ex.Message}");
            }
        }

        private static async Task<IResult> GetTagsDropdown()
        {
            try
            {
                // For now, return a simple list - this should be enhanced later
                var dropdownOptions = new List<DropdownOption>
                {
                    new() { Value = "1", Text = "Web Development" },
                    new() { Value = "2", Text = "Mobile Development" },
                    new() { Value = "3", Text = "Data Analysis" },
                    new() { Value = "4", Text = "Machine Learning" },
                    new() { Value = "5", Text = "UI/UX Design" }
                };

                return Results.Ok(dropdownOptions);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving tags dropdown: {ex.Message}");
            }
        }
    }

    public class DropdownOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
