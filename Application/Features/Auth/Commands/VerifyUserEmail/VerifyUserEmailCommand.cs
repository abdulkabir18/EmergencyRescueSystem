using Application.Common.Dtos;
using Application.Features.Auth.Dtos;
using MediatR;

namespace Application.Features.Auth.Commands.VerifyUserEmail
{
    public record VerifyUserEmailCommand(VerifyUserEmailRequestModel Model) : IRequest<Result<bool>>;
}
