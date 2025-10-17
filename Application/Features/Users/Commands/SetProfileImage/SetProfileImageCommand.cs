using Application.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Users.Commands.SetProfileImage
{
    public record SetProfileImageCommand(IFormFile Image) : IRequest<Result<Unit>>;
}
