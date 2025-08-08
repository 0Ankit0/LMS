using Microsoft.AspNetCore.Components.Forms;
using System.Threading.Tasks;

namespace LMS.Repositories
{
    public interface IImageRepository
    {
        Task<ImageUploadResult?> SaveImageAsync(IBrowserFile file);
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<string?> GetImageUrlAsync(string imageName);
    }

    public class ImageUploadResult
    {
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string? ContentType { get; set; }
    }

    public class ImageRepository : IImageRepository
    {
        private readonly string _imageFolder = "wwwroot/uploads/images";
        private readonly string _baseUrl = "/uploads/images/";

        public async Task<ImageUploadResult?> SaveImageAsync(IBrowserFile file)
        {
            if (file == null) return null;
            var fileName = $"{Guid.NewGuid()}_{file.Name}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), _imageFolder, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.OpenReadStream(10 * 1024 * 1024).CopyToAsync(stream);
            }
            return new ImageUploadResult
            {
                Url = _baseUrl + fileName,
                FileName = fileName,
                Size = file.Size,
                ContentType = file.ContentType
            };
        }

        public Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return Task.FromResult(false);
            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), _imageFolder, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<string?> GetImageUrlAsync(string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return Task.FromResult<string?>(null);
            var filePath = Path.Combine(_imageFolder, imageName);
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), filePath)))
            {
                return Task.FromResult(_baseUrl + imageName);
            }
            return Task.FromResult<string?>(null);
        }
    }
}
