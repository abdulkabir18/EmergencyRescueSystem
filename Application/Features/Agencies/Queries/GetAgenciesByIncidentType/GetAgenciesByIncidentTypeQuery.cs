using Application.Common.Dtos;
using Application.Features.Agencies.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Features.Agencies.Queries.GetAgenciesByIncidentType
{
    public record GetAgenciesByIncidentTypeQuery(IncidentType Type, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<AgencyDto>>;
}