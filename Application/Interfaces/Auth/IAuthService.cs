namespace Application.Interfaces.Auth
{
    public interface IAuthService
    {
        string GenerateToken(Guid userId, string email, string role);
    }
}
