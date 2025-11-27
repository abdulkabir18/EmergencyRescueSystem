using Application.Common.Dtos;
using Application.Features.Incidents.Dtos;
using MediatR;

namespace Application.Features.Incidents.Queries.GetAgencyIncidents
{
    public record GetAgencyIncidentsQuery(Guid AgencyId) : IRequest<Result<List<IncidentDto>>>;
}
