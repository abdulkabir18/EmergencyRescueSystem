namespace Application.Interfaces.External
{
    public interface IStorageManager
    {
        Task<string> UploadProfileImageAsync(Stream fileStream, string fileName, string contentType);
        Task DeleteProfileImageAsync(string fileUrl);

        Task<string> UploadMediaAsync(Stream fileStream, string fileName, string contentType);
        Task DeleteMediaAsync(string fileUrl);
    }
}
