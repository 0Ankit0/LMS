using LMS.Data.DTOs.FileManagement;

namespace LMS.Web.Repositories
{
    public interface IFileRepository
    {
        Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string contentType, string category = "general");
        Task<UserFileModel?> GetFileAsync(int fileId);
        Task<List<UserFileModel>> GetFilesByUserAsync(string userId);
        Task<List<UserFileModel>> GetFilesByCategoryAsync(string category);
        Task<bool> DeleteFileAsync(int fileId);
        Task<Stream?> DownloadFileAsync(int fileId);
        Task<bool> UpdateFileMetadataAsync(int fileId, string? description = null, bool? isPublic = null);
        Task<long> GetUserStorageUsageAsync(string userId);
        Task<bool> ValidateFileTypeAsync(string fileName, string contentType);
    }
}
