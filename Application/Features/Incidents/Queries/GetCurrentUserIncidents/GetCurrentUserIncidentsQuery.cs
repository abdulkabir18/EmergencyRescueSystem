using Application.Common.Dtos;
using Application.Features.Incidents.Dtos;
using MediatR;

namespace Application.Features.Incidents.Queries.GetCurrentUserIncidents
{
    public record GetCurrentUserIncidentsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<IncidentDto>>;
}