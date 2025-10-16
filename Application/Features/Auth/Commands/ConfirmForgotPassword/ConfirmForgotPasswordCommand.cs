using Application.Common.Dtos;
using Application.Features.Auth.Dtos;
using MediatR;

namespace Application.Features.Auth.Commands.ConfirmForgotPassword
{
    public record ConfirmForgotPasswordCommand(ConfirmForgotPasswordRequestModel Model) : IRequest<Result<bool>>;
}