using Application.Common.Dtos;
using Application.Features.Auth.Dtos;
using MediatR;

namespace Application.Features.Auth.Commands.Login
{
    public record LoginCommand(LoginRequestModel Model) : IRequest<Result<LoginResponseModel>>;
}