using LMS.Data.DTOs.LMS.Note;
using LMS.Data.Entities;
using LMS.Web.Infrastructure;
using LMS.Web.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LMS.Web.Endpoints
{
    public class NotesEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/notes").WithTags("Notes");
            var lessonsGroup = app.MapGroup("/api/lessons").WithTags("Notes");

            // Notes endpoints
            group.MapGet("", GetUserNotes)
                .RequireAuthorization()
                .WithName("GetUserNotes")
                .WithSummary("Get notes for the current user");

            group.MapPost("", CreateNote)
                .RequireAuthorization()
                .WithName("CreateNote")
                .WithSummary("Create a new note");

            group.MapPut("/{id:int}", UpdateNote)
                .RequireAuthorization()
                .WithName("UpdateNote")
                .WithSummary("Update an existing note");

            group.MapDelete("/{id:int}", DeleteNote)
                .RequireAuthorization()
                .WithName("DeleteNote")
                .WithSummary("Delete a note");

            group.MapGet("/{id:int}", GetNoteById)
                .RequireAuthorization()
                .WithName("GetNoteById")
                .WithSummary("Get a specific note by ID");

            // Lesson notes endpoints
            lessonsGroup.MapGet("/{lessonId:int}/notes", GetLessonNotes)
                .RequireAuthorization()
                .WithName("GetLessonNotes")
                .WithSummary("Get all notes for a specific lesson for the current user");
        }

        private static async Task<IResult> GetUserNotes(
            INoteRepository noteRepository,
            ClaimsPrincipal user,
            int? courseId = null,
            int? lessonId = null,
            string? searchTerm = null,
            string? tag = null,
            string? priority = null,
            bool? isPinned = null,
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

                var filter = new NoteFilterDTO
                {
                    CourseId = courseId,
                    LessonId = lessonId,
                    SearchTerm = searchTerm,
                    Tags = tag,
                    Priority = !string.IsNullOrEmpty(priority) && Enum.TryParse<NotePriority>(priority, true, out var priorityEnum) ? priorityEnum : null,
                    IsPinned = isPinned,
                    Page = page,
                    PageSize = pageSize
                };

                var notes = await noteRepository.GetUserNotesAsync(userId, filter);
                return Results.Ok(notes);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving user notes: {ex.Message}");
            }
        }

        private static async Task<IResult> GetLessonNotes(
            int lessonId,
            INoteRepository noteRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var notes = await noteRepository.GetNotesForLessonAsync(userId, lessonId);
                return Results.Ok(notes);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving lesson notes: {ex.Message}");
            }
        }

        private static async Task<IResult> CreateNote(
            CreateNoteRequest request,
            INoteRepository noteRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var note = await noteRepository.CreateNoteAsync(request, userId);
                return Results.CreatedAtRoute("GetNoteById", new { id = note.Id }, note);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating note: {ex.Message}");
            }
        }

        private static async Task<IResult> UpdateNote(
            int id,
            UpdateNoteRequest request,
            INoteRepository noteRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                // Check if user owns the note
                var existingNote = await noteRepository.GetNoteByIdAsync(id, userId);
                if (existingNote == null)
                {
                    return Results.NotFound();
                }

                if (existingNote.UserId != userId)
                {
                    return Results.Forbid();
                }

                var note = await noteRepository.UpdateNoteAsync(request, userId);
                if (note == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(note);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating note: {ex.Message}");
            }
        }

        private static async Task<IResult> DeleteNote(
            int id,
            INoteRepository noteRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                // Check if user owns the note
                var existingNote = await noteRepository.GetNoteByIdAsync(id, userId);
                if (existingNote == null)
                {
                    return Results.NotFound();
                }

                if (existingNote.UserId != userId)
                {
                    return Results.Forbid();
                }

                var success = await noteRepository.DeleteNoteAsync(id, userId);
                if (!success)
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting note: {ex.Message}");
            }
        }

        private static async Task<IResult> GetNoteById(
            int id,
            INoteRepository noteRepository,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var note = await noteRepository.GetNoteByIdAsync(id, userId);
                if (note == null)
                {
                    return Results.NotFound();
                }

                // Check if user owns the note
                if (note.UserId != userId)
                {
                    return Results.Forbid();
                }

                return Results.Ok(note);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving note: {ex.Message}");
            }
        }
    }
}
