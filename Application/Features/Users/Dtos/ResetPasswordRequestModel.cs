namespace Application.Features.Users.Dtos
{
    public record ResetPasswordRequestModel(string CurrentPassword, string NewPassword, string ConfirmPassword);
}
