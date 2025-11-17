using Application.Common.Dtos;
using MediatR;

namespace Application.Features.Incidents.Commands.AcceptIncident
{
    public record AcceptIncidentCommand(Guid IncidentId) : IRequest<Result<Guid>>;
}