using Application.Common.Dtos;
using Application.Features.Incidents.Dtos;
using MediatR;

namespace Application.Features.Incidents.Commands.CreateIncident
{
    public record CreateIncidentCommand(CreateIncidentRequestModel Model) : IRequest<Result<Guid>>;
}