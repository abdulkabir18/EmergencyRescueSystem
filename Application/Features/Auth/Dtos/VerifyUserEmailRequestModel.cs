namespace Application.Features.Auth.Dtos
{
    public record VerifyUserEmailRequestModel(string Email, string Code);
}