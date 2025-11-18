using Application.Common.Dtos;
using MediatR;

namespace Application.Features.Incidents.Commands.CancelIncident
{
    public record CancelIncidentCommand(Guid IncidentId) : IRequest<Result<Guid>>;
}