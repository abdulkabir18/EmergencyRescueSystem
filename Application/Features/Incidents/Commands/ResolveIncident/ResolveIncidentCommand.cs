using Application.Common.Dtos;
using MediatR;

namespace Application.Features.Incidents.Commands.ResolveIncident
{
    public record ResolveIncidentCommand(Guid IncidentId) : IRequest<Result<Guid>>;
}
