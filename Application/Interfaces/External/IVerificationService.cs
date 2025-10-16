namespace Application.Interfaces.External
{
    public interface IVerificationService
    {
        Task<string> GenerateCodeAsync(Guid userId, int expiryMinutes = 15);
        Task<bool> ValidateCodeAsync(Guid userId, string code);
        Task<bool> CanRequestNewCodeAsync(Guid userId);
    }
}
