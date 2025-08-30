using LMS.Web.DTOs.FileManagement;
using LMS.Data.Entities;
using LMS.Web.Data;
using LMS.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMS.Web.Endpoints
{
    public class FileManagementEndpoints : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/files").WithTags("File Management");

            // File upload endpoints
            group.MapPost("/upload", UploadFile)
                .RequireAuthorization()
                .WithName("UploadFile")
                .WithSummary("Upload a file")
                .DisableAntiforgery();

            group.MapPost("/upload/multiple", UploadMultipleFiles)
                .RequireAuthorization()
                .WithName("UploadMultipleFiles")
                .WithSummary("Upload multiple files")
                .DisableAntiforgery();

            // File retrieval endpoints
            group.MapGet("/{fileId:int}", GetFileDetails)
                .RequireAuthorization()
                .WithName("GetFileDetails")
                .WithSummary("Get file details by ID");

            group.MapGet("/{fileId:int}/download", DownloadFile)
                .RequireAuthorization()
                .WithName("DownloadFile")
                .WithSummary("Download a file");

            group.MapGet("/user/{userId}", GetUserFiles)
                .RequireAuthorization()
                .WithName("GetUserFiles")
                .WithSummary("Get files uploaded by a specific user");

            group.MapPost("/search", SearchFiles)
                .RequireAuthorization()
                .WithName("SearchFiles")
                .WithSummary("Search files with filters");

            // File management endpoints
            group.MapPut("/{fileId:int}", UpdateFileDetails)
                .RequireAuthorization()
                .WithName("UpdateFileDetails")
                .WithSummary("Update file details");

            group.MapDelete("/{fileId:int}", DeleteFile)
                .RequireAuthorization()
                .WithName("DeleteFile")
                .WithSummary("Delete a file");

            group.MapPost("/bulk-operations", BulkFileOperations)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .WithName("BulkFileOperations")
                .WithSummary("Perform bulk operations on files");

            // Public file access (for thumbnails, profile pictures, etc.)
            group.MapGet("/public/{fileId:int}", GetPublicFile)
                .AllowAnonymous()
                .WithName("GetPublicFile")
                .WithSummary("Get public file (no authentication required)");
        }

        private static async Task<IResult> UploadFile(
            HttpRequest request,
            ApplicationDbContext context,
            ClaimsPrincipal user,
            IWebHostEnvironment environment)
        {
            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            if (!request.HasFormContentType)
                return Results.BadRequest("Request must be multipart/form-data");

            var form = await request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
                return Results.BadRequest("No file provided");

            // Validate file size (max 100MB)
            if (file.Length > 100 * 1024 * 1024)
                return Results.BadRequest("File size exceeds maximum limit of 100MB");

            // Create upload directory if it doesn't exist
            var uploadsPath = Path.Combine(environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Generate unique file name
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save file info to database
            var userFile = new UserFile
            {
                OwnerUserId = userId,
                FileName = file.FileName,
                FilePath = $"/uploads/{uniqueFileName}",
                FileSize = file.Length,
                ContentType = file.ContentType ?? "application/octet-stream"
            };

            context.UserFiles.Add(userFile);
            await context.SaveChangesAsync();

            var result = new FileUploadResult
            {
                Success = true,
                FileId = userFile.Id,
                FileName = userFile.FileName,
                FilePath = userFile.FilePath,
                FileUrl = $"/api/files/{userFile.Id}/download",
                FileSize = userFile.FileSize
            };

            return Results.Ok(result);
        }

        private static async Task<IResult> UploadMultipleFiles(
            HttpRequest request,
            ApplicationDbContext context,
            ClaimsPrincipal user,
            IWebHostEnvironment environment)
        {
            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            if (!request.HasFormContentType)
                return Results.BadRequest("Request must be multipart/form-data");

            var form = await request.ReadFormAsync();
            var files = form.Files;

            if (!files.Any())
                return Results.BadRequest("No files provided");

            var results = new List<FileUploadResult>();
            var uploadsPath = Path.Combine(environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            foreach (var file in files)
            {
                try
                {
                    if (file.Length == 0)
                        continue;

                    // Validate file size
                    if (file.Length > 100 * 1024 * 1024)
                    {
                        results.Add(new FileUploadResult
                        {
                            Success = false,
                            FileName = file.FileName,
                            ErrorMessage = "File size exceeds maximum limit of 100MB"
                        });
                        continue;
                    }

                    var extension = Path.GetExtension(file.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var userFile = new UserFile
                    {
                        OwnerUserId = userId,
                        FileName = file.FileName,
                        FilePath = $"/uploads/{uniqueFileName}",
                        FileSize = file.Length,
                        ContentType = file.ContentType ?? "application/octet-stream"
                    };

                    context.UserFiles.Add(userFile);
                    await context.SaveChangesAsync();

                    results.Add(new FileUploadResult
                    {
                        Success = true,
                        FileId = userFile.Id,
                        FileName = userFile.FileName,
                        FilePath = userFile.FilePath,
                        FileUrl = $"/api/files/{userFile.Id}/download",
                        FileSize = userFile.FileSize
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new FileUploadResult
                    {
                        Success = false,
                        FileName = file.FileName,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return Results.Ok(results);
        }

        private static async Task<IResult> GetFileDetails(
            int fileId,
            ApplicationDbContext context,
            ClaimsPrincipal user)
        {
            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = user.IsInRole("Admin");

            var file = await context.UserFiles
                .Include(f => f.Owner)
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
                return Results.NotFound();

            // Check if user has access to this file
            if (!isAdmin && file.OwnerUserId != userId)
                return Results.Forbid();

            var model = new UserFileModel
            {
                Id = file.Id,
                OwnerUserId = file.OwnerUserId,
                OwnerUserName = file.Owner.UserName ?? string.Empty,
                FileName = file.FileName,
                FilePath = file.FilePath,
                FileSize = file.FileSize,
                ContentType = file.ContentType,
                UploadedAt = file.UploadedAt,
                IsActive = file.IsActive,
                FileSizeFormatted = FormatFileSize(file.FileSize),
                FileUrl = $"/api/files/{file.Id}/download"
            };

            return Results.Ok(model);
        }

        private static async Task<IResult> DownloadFile(
            int fileId,
            ApplicationDbContext context,
            ClaimsPrincipal user,
            IWebHostEnvironment environment)
        {
            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = user.IsInRole("Admin");

            var file = await context.UserFiles.FindAsync(fileId);
            if (file == null)
                return Results.NotFound();

            // Check if user has access to this file
            if (!isAdmin && file.OwnerUserId != userId)
                return Results.Forbid();

            var physicalPath = Path.Combine(environment.WebRootPath, file.FilePath.TrimStart('/'));

            if (!File.Exists(physicalPath))
                return Results.NotFound("File not found on disk");

            var fileBytes = await File.ReadAllBytesAsync(physicalPath);

            return Results.File(fileBytes, file.ContentType, file.FileName);
        }

        private static async Task<IResult> GetUserFiles(
            string userId,
            ApplicationDbContext context,
            ClaimsPrincipal user,
            int page = 1,
            int pageSize = 20)
        {
            var currentUserId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = user.IsInRole("Admin");

            // Check if user has access to these files
            if (!isAdmin && currentUserId != userId)
                return Results.Forbid();

            var skip = (page - 1) * pageSize;

            var files = await context.UserFiles
                .Where(f => f.OwnerUserId == userId && f.IsActive)
                .Include(f => f.Owner)
                .OrderByDescending(f => f.UploadedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(f => new UserFileModel
                {
                    Id = f.Id,
                    OwnerUserId = f.OwnerUserId,
                    OwnerUserName = f.Owner.UserName ?? string.Empty,
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    FileSize = f.FileSize,
                    ContentType = f.ContentType,
                    UploadedAt = f.UploadedAt,
                    IsActive = f.IsActive,
                    FileSizeFormatted = FormatFileSize(f.FileSize),
                    FileUrl = $"/api/files/{f.Id}/download"
                })
                .ToListAsync();

            var totalCount = await context.UserFiles
                .CountAsync(f => f.OwnerUserId == userId && f.IsActive);

            var result = new
            {
                Data = files,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Results.Ok(result);
        }

        private static async Task<IResult> SearchFiles(
            FileSearchRequest request,
            ApplicationDbContext context,
            ClaimsPrincipal user)
        {
            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = user.IsInRole("Admin");

            var query = context.UserFiles
                .Include(f => f.Owner)
                .Where(f => f.IsActive);

            // If not admin, only show user's own files
            if (!isAdmin)
                query = query.Where(f => f.OwnerUserId == userId);

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(f => f.FileName.Contains(request.SearchTerm));
            }

            if (!string.IsNullOrEmpty(request.ContentType))
            {
                query = query.Where(f => f.ContentType.Contains(request.ContentType));
            }

            if (!string.IsNullOrEmpty(request.OwnerUserId) && isAdmin)
            {
                query = query.Where(f => f.OwnerUserId == request.OwnerUserId);
            }

            if (request.UploadedAfter.HasValue)
            {
                query = query.Where(f => f.UploadedAt >= request.UploadedAfter.Value);
            }

            if (request.UploadedBefore.HasValue)
            {
                query = query.Where(f => f.UploadedAt <= request.UploadedBefore.Value);
            }

            if (request.MinSize.HasValue)
            {
                query = query.Where(f => f.FileSize >= request.MinSize.Value);
            }

            if (request.MaxSize.HasValue)
            {
                query = query.Where(f => f.FileSize <= request.MaxSize.Value);
            }

            var totalCount = await query.CountAsync();
            var skip = (request.Page - 1) * request.PageSize;

            var files = await query
                .OrderByDescending(f => f.UploadedAt)
                .Skip(skip)
                .Take(request.PageSize)
                .Select(f => new UserFileModel
                {
                    Id = f.Id,
                    OwnerUserId = f.OwnerUserId,
                    OwnerUserName = f.Owner.UserName ?? string.Empty,
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    FileSize = f.FileSize,
                    ContentType = f.ContentType,
                    UploadedAt = f.UploadedAt,
                    IsActive = f.IsActive,
                    FileSizeFormatted = FormatFileSize(f.FileSize),
                    FileUrl = $"/api/files/{f.Id}/download"
                })
                .ToListAsync();

            var result = new
            {
                Data = files,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Results.Ok(result);
        }

        private static async Task<IResult> UpdateFileDetails(
            int fileId,
            UserFileModel request,
            ApplicationDbContext context,
            ClaimsPrincipal user)
        {
            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = user.IsInRole("Admin");

            var file = await context.UserFiles.FindAsync(fileId);
            if (file == null)
                return Results.NotFound();

            // Check if user has access to this file
            if (!isAdmin && file.OwnerUserId != userId)
                return Results.Forbid();

            if (!string.IsNullOrEmpty(request.FileName))
                file.FileName = request.FileName;

            await context.SaveChangesAsync();

            return Results.NoContent();
        }

        private static async Task<IResult> DeleteFile(
            int fileId,
            ApplicationDbContext context,
            ClaimsPrincipal user,
            IWebHostEnvironment environment)
        {
            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = user.IsInRole("Admin");

            var file = await context.UserFiles.FindAsync(fileId);
            if (file == null)
                return Results.NotFound();

            // Check if user has access to this file
            if (!isAdmin && file.OwnerUserId != userId)
                return Results.Forbid();

            // Mark as inactive instead of deleting to preserve referential integrity
            file.IsActive = false;
            await context.SaveChangesAsync();

            // Optionally delete physical file
            var physicalPath = Path.Combine(environment.WebRootPath, file.FilePath.TrimStart('/'));
            if (File.Exists(physicalPath))
            {
                try
                {
                    File.Delete(physicalPath);
                }
                catch (Exception)
                {
                    // Log error but don't fail the request
                }
            }

            return Results.NoContent();
        }

        private static async Task<IResult> BulkFileOperations(
            BulkFileOperationRequest request,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            var result = new BulkFileOperationResult();
            var files = await context.UserFiles
                .Where(f => request.FileIds.Contains(f.Id))
                .ToListAsync();

            foreach (var file in files)
            {
                try
                {
                    switch (request.Operation.ToLower())
                    {
                        case "delete":
                            file.IsActive = false;
                            // Optionally delete physical file
                            var physicalPath = Path.Combine(environment.WebRootPath, file.FilePath.TrimStart('/'));
                            if (File.Exists(physicalPath))
                                File.Delete(physicalPath);
                            break;

                        case "activate":
                            file.IsActive = true;
                            break;

                        case "deactivate":
                            file.IsActive = false;
                            break;

                        default:
                            result.ErrorMessages.Add($"Unknown operation: {request.Operation}");
                            result.FailedCount++;
                            continue;
                    }

                    result.ProcessedCount++;
                }
                catch (Exception ex)
                {
                    result.ErrorMessages.Add($"Error processing file {file.Id}: {ex.Message}");
                    result.FailedCount++;
                }
            }

            if (result.ProcessedCount > 0)
            {
                await context.SaveChangesAsync();
            }

            result.Success = result.FailedCount == 0;

            return Results.Ok(result);
        }

        private static async Task<IResult> GetPublicFile(
            int fileId,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            var file = await context.UserFiles.FindAsync(fileId);
            if (file == null || !file.IsActive)
                return Results.NotFound();

            var physicalPath = Path.Combine(environment.WebRootPath, file.FilePath.TrimStart('/'));

            if (!File.Exists(physicalPath))
                return Results.NotFound("File not found on disk");

            var fileBytes = await File.ReadAllBytesAsync(physicalPath);

            return Results.File(fileBytes, file.ContentType, file.FileName);
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
