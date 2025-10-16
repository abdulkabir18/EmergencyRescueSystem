using Application.Interfaces.External;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Storage
{
    public class CloudinaryStorageService : IStorageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryStorageService> _logger;

        public CloudinaryStorageService(IConfiguration configuration, ILogger<CloudinaryStorageService> logger)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
        {
            string resourceType = GetResourceType(contentType);

            try
            {
                dynamic uploadParams;
                dynamic uploadResult;

                switch (resourceType)
                {
                    case "image":
                        uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription(fileName, fileStream),
                            UseFilename = true,
                            UniqueFilename = true,
                            Overwrite = false,
                            Folder = "naijarescue"
                        };
                        uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        break;
                    case "video":
                        uploadParams = new VideoUploadParams
                        {
                            File = new FileDescription(fileName, fileStream),
                            UseFilename = true,
                            UniqueFilename = true,
                            Overwrite = false,
                            Folder = "naijarescue"
                        };
                        uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        break;
                    default:
                        uploadParams = new RawUploadParams
                        {
                            File = new FileDescription(fileName, fileStream),
                            UseFilename = true,
                            UniqueFilename = true,
                            Overwrite = false,
                            Folder = "naijarescue"
                        };
                        uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        break;
                }

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK || uploadResult.SecureUrl == null)
                {
                    _logger.LogError("Cloudinary upload failed for file {FileName}. Error: {Error}", fileName, (object?)uploadResult.Error?.Message);
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error?.Message}");
                }

                _logger.LogInformation("File {FileName} uploaded successfully to Cloudinary. URL: {Url}", fileName, (object?)uploadResult.SecureUrl.AbsoluteUri);
                return uploadResult.SecureUrl.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while uploading file {FileName} to Cloudinary.", fileName);
                throw;
            }
        }

        public async Task DeleteAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                _logger.LogWarning("Attempted to delete a Cloudinary file with an empty URL.");
                return;
            }

            var publicId = GetPublicIdFromUrl(fileUrl);
            if (string.IsNullOrWhiteSpace(publicId))
            {
                _logger.LogWarning("Failed to extract publicId from URL: {Url}", fileUrl);
                return;
            }

            try
            {
                var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));

                if (result.Result == "ok" || result.Result == "not found")
                {
                    _logger.LogInformation("Cloudinary file deleted successfully. PublicId: {PublicId}, Result: {Result}", publicId, result.Result);
                }
                else
                {
                    _logger.LogError("Failed to delete file from Cloudinary. PublicId: {PublicId}, Error: {Error}", publicId, result.Error?.Message);
                    throw new Exception($"Failed to delete file from Cloudinary: {result.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting Cloudinary file. URL: {Url}", fileUrl);
                throw;
            }
        }

        private string GetResourceType(string contentType)
        {
            if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return "image";
            if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                return "video";
            return "raw";
        }

        private string GetPublicIdFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var parts = uri.AbsolutePath.Split('/');
                var uploadIndex = Array.IndexOf(parts, "upload");
                if (uploadIndex == -1 || uploadIndex + 2 >= parts.Length)
                    return string.Empty;

                var relevantParts = parts.Skip(uploadIndex + 2).ToList();
                if (relevantParts.Count > 0)
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(relevantParts.Last());
                    relevantParts[relevantParts.Count - 1] = fileNameWithoutExt;
                }

                return string.Join("/", relevantParts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse publicId from Cloudinary URL: {Url}", url);
                return string.Empty;
            }
        }
    }
}