namespace Application.Features.Auth.Dtos
{
    public record LoginRequestModel(string Email, string Password);
    public record LoginResponseModel(string Token, bool IsEmailVerified = false);
}