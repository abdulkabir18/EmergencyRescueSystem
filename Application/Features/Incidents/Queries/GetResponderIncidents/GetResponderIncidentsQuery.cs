using Application.Common.Dtos;
using Application.Features.Incidents.Dtos;
using MediatR;

namespace Application.Features.Incidents.Queries.GetResponderIncidents
{
    public record GetResponderIncidentsQuery(Guid ResponderId) : IRequest<Result<List<IncidentDto>>>;
}