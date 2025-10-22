using Application.Common.Dtos;
using MediatR;

namespace Application.Features.Auth.Commands.ResendVerificationCode
{
    public record ResendVerificationCodeCommand(string Email) : IRequest<Result<bool>>;
}
