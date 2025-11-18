using Application.Common.Dtos;
using MediatR;

namespace Application.Features.Incidents.Commands.MarkInProgress
{
    public record MarkIncidentInProgressCommand(Guid IncidentId) : IRequest<Result<Guid>>;
}
