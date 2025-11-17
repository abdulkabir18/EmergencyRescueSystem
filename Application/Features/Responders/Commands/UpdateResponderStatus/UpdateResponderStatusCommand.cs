using Application.Common.Dtos;
using Application.Features.Responders.Dtos;
using MediatR;

namespace Application.Features.Responders.Commands.UpdateResponderStatus
{
    public record UpdateResponderStatusCommand(Guid ResponderId, UpdateResponderStatusRequestModel Model) : IRequest<Result<Unit>>;
}