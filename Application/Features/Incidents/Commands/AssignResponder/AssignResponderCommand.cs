using Application.Common.Dtos;
using Application.Features.Incidents.Dtos;
using MediatR;

namespace Application.Features.Incidents.Commands.AssignResponder
{
    public record AssignResponderCommand(AssignResponderRequestModel Model) : IRequest<Result<Guid>>;
}
