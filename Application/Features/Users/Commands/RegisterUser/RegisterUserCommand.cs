using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using MediatR;

namespace Application.Features.Users.Commands.RegisterUser
{
    public record RegisterUserCommand(RegisterUserRequestModel Model) : IRequest<Result<Guid>>;
}
