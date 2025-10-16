using Application.Interfaces.External;

namespace Infrastructure.Services.Storage.Manager
{
    public class StorageManager : IStorageManager
    {
        private readonly CloudinaryStorageService _cloudinary;
        private readonly LocalStorageService _local;

        public StorageManager(CloudinaryStorageService cloudinary, LocalStorageService local)
        {
            _cloudinary = cloudinary;
            _local = local;
        }

        public Task<string> UploadProfileImageAsync(Stream fileStream, string fileName, string contentType)
            => _local.UploadAsync(fileStream, fileName, contentType);

        public Task DeleteProfileImageAsync(string fileUrl)
            => _local.DeleteAsync(fileUrl);

        public Task<string> UploadMediaAsync(Stream fileStream, string fileName, string contentType)
            => _cloudinary.UploadAsync(fileStream, fileName, contentType);

        public Task DeleteMediaAsync(string fileUrl)
            => _cloudinary.DeleteAsync(fileUrl);
    }
}
