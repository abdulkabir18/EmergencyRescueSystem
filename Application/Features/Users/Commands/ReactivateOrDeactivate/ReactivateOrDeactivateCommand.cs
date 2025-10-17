using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using MediatR;

namespace Application.Features.Users.Commands.ReactivateOrDeactivate
{
    public record ReactivateOrDeactivateCommand(ReactivateOrDeactivateRequestModel Model) : IRequest<Result<Unit>>;
}
