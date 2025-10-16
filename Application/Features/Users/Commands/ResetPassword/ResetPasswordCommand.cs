using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using MediatR;

namespace Application.Features.Users.Commands.ResetPassword
{
    public record ResetPasswordCommand(ResetPasswordRequestModel Model) : IRequest<Result<bool>>;
}
