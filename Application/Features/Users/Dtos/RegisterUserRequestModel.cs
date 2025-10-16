using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Users.Dtos
{
    public record RegisterUserRequestModel(string FirstName, string LastName, string Email,Gender Gender, string Password, string ConfirmPassword, IFormFile? ProfilePicture);
}