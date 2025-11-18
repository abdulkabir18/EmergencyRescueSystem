using Application.Common.Dtos;
using Application.Features.Agencies.Dtos;
using MediatR;

namespace Application.Features.Agencies.Queries.GetAllAgencies
{
    public record GetAllAgenciesQuery(GetAllAgenciesRequest Model) : IRequest<PaginatedResult<AgencyDto>>;
}
