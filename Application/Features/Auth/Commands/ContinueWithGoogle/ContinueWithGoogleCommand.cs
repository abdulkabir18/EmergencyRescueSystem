using Application.Common.Dtos;
using Application.Features.Auth.Dtos;
using Application.Features.Users.Dtos;
using MediatR;

namespace Application.Features.Auth.Commands.ContinueWithGoogle
{
    public record ContinueWithGoogleCommand(GoogleLoginRequestModel Model) : IRequest<Result<LoginResponseModel>>;
}
