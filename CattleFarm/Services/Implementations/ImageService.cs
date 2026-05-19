using CattleFarm.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace CattleFarm.Services.Implementations
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public ImageService(IWebHostEnvironment env) { _env = env; }

        public bool IsValidImage(IFormFile? file)
        {
            if (file is null || file.Length == 0) return false;
            if (file.Length > MaxFileSizeBytes) return false;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(ext);
        }

        public async Task<string?> SaveImageAsync(IFormFile? file, string folder)
        {
            if (file is null || file.Length == 0) return null;
            if (!IsValidImage(file)) return null;
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsDir);
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);
            using var image = await Image.LoadAsync(file.OpenReadStream());
            if (image.Width > 1200) image.Mutate(x => x.Resize(1200, 0));
            await image.SaveAsync(fullPath);
            return $"/uploads/{folder}/{fileName}";
        }

        public void DeleteImage(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;
            var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
    }
}
