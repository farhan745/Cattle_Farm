using CattleFarm.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace CattleFarm.Services.Implementations
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly string[] UploadExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf" };
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"
        };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public ImageService(IWebHostEnvironment env) { _env = env; }

        private string WebRootPath =>
            _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

        private static string SanitizeFolder(string folder)
        {
            var safe = Path.GetFileName(folder.Trim().Trim('/', '\\'));
            return string.IsNullOrEmpty(safe) ? "misc" : safe;
        }

        public bool IsValidImage(IFormFile? file)
        {
            if (file is null || file.Length == 0) return false;
            if (file.Length > MaxFileSizeBytes) return false;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext)) return false;

            if (!string.IsNullOrWhiteSpace(file.ContentType) && !AllowedContentTypes.Contains(file.ContentType))
                return false;

            try
            {
                using var stream = file.OpenReadStream();
                return Image.Identify(stream) is not null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> SaveImageAsync(IFormFile? file, string folder)
        {
            if (file is null || file.Length == 0) return null;
            if (!IsValidImage(file)) return null;

            var safeFolder = SanitizeFolder(folder);
            var uploadsDir = Path.Combine(WebRootPath, "uploads", safeFolder);
            Directory.CreateDirectory(uploadsDir);
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            try
            {
                await using var stream = file.OpenReadStream();
                using var image = await Image.LoadAsync(stream);
                if (image.Width > 1200)
                    image.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(1200, 0), Mode = ResizeMode.Max }));
                await image.SaveAsync(fullPath);
                return $"/uploads/{safeFolder}/{fileName}";
            }
            catch (UnknownImageFormatException)
            {
                return null;
            }
            catch (InvalidImageContentException)
            {
                return null;
            }
        }

        public async Task<string?> SaveUploadAsync(IFormFile? file, string folder)
        {
            if (file is null || file.Length == 0) return null;
            if (file.Length > MaxFileSizeBytes) return null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!UploadExtensions.Contains(ext)) return null;

            if (AllowedExtensions.Contains(ext))
                return await SaveImageAsync(file, folder);

            var safeFolder = SanitizeFolder(folder);
            var uploadsDir = Path.Combine(WebRootPath, "uploads", safeFolder);
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);
            return $"/uploads/{safeFolder}/{fileName}";
        }

        public void DeleteImage(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;
            var fullPath = Path.Combine(WebRootPath, relativePath.TrimStart('/'));
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
    }
}
