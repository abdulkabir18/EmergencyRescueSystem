using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using MediatR;

namespace Application.Features.Users.Commands.UpdateFullName
{
    public record UpdateFullNameCommand(UpdateFullNameRequestModel Model) : IRequest<Result<Unit>>;
}
