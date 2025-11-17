using Application.Common.Dtos;
using Application.Features.Responders.Dtos;
using MediatR;

namespace Application.Features.Responders.Commands.UpdateResponderLocation
{
    public record UpdateResponderLocationCommand(Guid ResponderId, UpdateResponderLocationRequestModel Model) : IRequest<Result<Unit>>;
}