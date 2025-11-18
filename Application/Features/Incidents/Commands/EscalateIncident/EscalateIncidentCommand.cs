using Application.Common.Dtos;
using MediatR;

namespace Application.Features.Incidents.Commands.EscalateIncident
{
    public record EscalateIncidentCommand(Guid IncidentId) : IRequest<Result<Guid>>;
}
