using LMS.Data.DTOs;
using LMS.Data.Entities;
using LMS.Web.Infrastructure;
using LMS.Web.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LMS.Web.Endpoints
{
    public class NotificationsCalendarEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var notificationsGroup = app.MapGroup("/api/notifications").WithTags("Notifications");
            var calendarGroup = app.MapGroup("/api/calendar").WithTags("Calendar");
            var calendarEventsGroup = app.MapGroup("/api/calendar-events").WithTags("Calendar");

            // Notifications endpoints
            notificationsGroup.MapGet("", GetNotifications)
                .RequireAuthorization()
                .WithName("GetNotifications")
                .WithSummary("Get all notifications for the current user");

            notificationsGroup.MapPost("/{id:int}/mark-as-read", MarkNotificationAsRead)
                .RequireAuthorization()
                .WithName("MarkNotificationAsRead")
                .WithSummary("Mark a specific notification as read");

            notificationsGroup.MapPost("/mark-all-read", MarkAllNotificationsAsRead)
                .RequireAuthorization()
                .WithName("MarkAllNotificationsAsRead")
                .WithSummary("Mark all notifications as read for the current user");

            notificationsGroup.MapGet("/unread-count", GetUnreadNotificationsCount)
                .RequireAuthorization()
                .WithName("GetUnreadNotificationsCount")
                .WithSummary("Get count of unread notifications");

            // Calendar endpoints
            calendarGroup.MapGet("", GetCalendarEvents)
                .RequireAuthorization()
                .WithName("GetCalendarEvents")
                .WithSummary("Get all calendar events for the current user for a given date range");

            calendarGroup.MapGet("/upcoming", GetUpcomingEvents)
                .RequireAuthorization()
                .WithName("GetUpcomingEvents")
                .WithSummary("Get upcoming calendar events");

            // Calendar Events endpoints
            calendarEventsGroup.MapPost("", CreateCalendarEvent)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("CreateCalendarEvent")
                .WithSummary("Create a new custom calendar event for a course");

            calendarEventsGroup.MapPut("/{id:int}", UpdateCalendarEvent)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("UpdateCalendarEvent")
                .WithSummary("Update a calendar event");

            calendarEventsGroup.MapDelete("/{id:int}", DeleteCalendarEvent)
                .RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"))
                .WithName("DeleteCalendarEvent")
                .WithSummary("Delete a calendar event");
        }

        // Notifications methods
        private static async Task<IResult> GetNotifications(
            IDashboardRepository dashboardRepository,
            ClaimsPrincipal user,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var notifications = await dashboardRepository.GetUserNotificationsAsync(userId, page, pageSize);
                return Results.Ok(notifications);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving notifications: {ex.Message}");
            }
        }

        private static async Task<IResult> MarkNotificationAsRead(
            int id,
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

                var success = await dashboardRepository.MarkNotificationAsReadAsync(id, userId);
                if (!success)
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error marking notification as read: {ex.Message}");
            }
        }

        private static async Task<IResult> MarkAllNotificationsAsRead(
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

                await dashboardRepository.MarkAllNotificationsAsReadAsync(userId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error marking all notifications as read: {ex.Message}");
            }
        }

        private static async Task<IResult> GetUnreadNotificationsCount(
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

                var count = await dashboardRepository.GetUnreadNotificationsCountAsync(userId);
                return Results.Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error getting unread notifications count: {ex.Message}");
            }
        }

        // Calendar methods
        private static async Task<IResult> GetCalendarEvents(
            IDashboardRepository dashboardRepository,
            ClaimsPrincipal user,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                // Default to current month if no dates provided
                startDate ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                endDate ??= startDate.Value.AddMonths(1).AddDays(-1);

                var events = await dashboardRepository.GetUserCalendarEventsAsync(userId, startDate.Value, endDate.Value);
                return Results.Ok(events);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving calendar events: {ex.Message}");
            }
        }

        private static async Task<IResult> GetUpcomingEvents(
            IDashboardRepository dashboardRepository,
            ClaimsPrincipal user,
            int days = 7)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var startDate = DateTime.Now;
                var endDate = DateTime.Now.AddDays(days);

                var events = await dashboardRepository.GetUserCalendarEventsAsync(userId, startDate, endDate);
                return Results.Ok(events);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving upcoming events: {ex.Message}");
            }
        }

        private static async Task<IResult> CreateCalendarEvent(
            CreateCalendarEventRequest request,
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

                var calendarEvent = await dashboardRepository.CreateCalendarEventAsync(request, userId);
                return Results.Ok(calendarEvent);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating calendar event: {ex.Message}");
            }
        }

        private static async Task<IResult> UpdateCalendarEvent(
            int id,
            UpdateCalendarEventRequest request,
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

                var calendarEvent = await dashboardRepository.UpdateCalendarEventAsync(id, request, userId);
                if (calendarEvent == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(calendarEvent);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating calendar event: {ex.Message}");
            }
        }

        private static async Task<IResult> DeleteCalendarEvent(
            int id,
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

                var success = await dashboardRepository.DeleteCalendarEventAsync(id, userId);
                if (!success)
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting calendar event: {ex.Message}");
            }
        }
    }
}
