using Application.Interfaces.External;

namespace Infrastructure.Services.Storage
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _basePath;
        private readonly string _profileFolder = "uploads/profile-pictures";

        public LocalStorageService(string rootPath)
        {
            _basePath = Path.Combine(rootPath, _profileFolder);

            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        public Task DeleteAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return Task.CompletedTask;

            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_basePath, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.CompletedTask;
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
        {
            var extension = Path.GetExtension(fileName);
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            if (!allowedExtensions.Contains(extension.ToLower()))
                throw new InvalidOperationException("Invalid file type.");
            if (fileStream.Length > 5 * 1024 * 1024)
               throw new InvalidOperationException("File size exceeds the 5MB limit.");
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_basePath, uniqueFileName);

            using (var fileStreamLocal = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamLocal);
            }

            var relativePath = $"/{_profileFolder}/{uniqueFileName}".Replace("\\", "/");
            return relativePath;
        }
    }
}
