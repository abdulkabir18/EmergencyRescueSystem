namespace Application.Interfaces.External
{
    public interface IStorageService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string folder);
        Task DeleteAsync(string fileUrl);
    }
}
