namespace CattleFarm.Services.Interfaces
{
    public interface IImageService
    {
        /// <summary>Saves an uploaded image to disk, returns the relative URL path.</summary>
        Task<string?> SaveImageAsync(IFormFile? file, string folder);

        /// <summary>Deletes an image file by its relative path.</summary>
        void DeleteImage(string? relativePath);

        /// <summary>Returns true if the file extension and MIME type are an allowed image type.</summary>
        bool IsValidImage(IFormFile? file);

        /// <summary>Saves an image or PDF (e.g. prescription). Returns relative URL path.</summary>
        Task<string?> SaveUploadAsync(IFormFile? file, string folder);
    }
}
