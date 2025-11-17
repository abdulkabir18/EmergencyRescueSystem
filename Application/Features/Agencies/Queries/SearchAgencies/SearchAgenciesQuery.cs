using Application.Common.Dtos;
using Application.Features.Agencies.Dtos;
using MediatR;

namespace Application.Features.Agencies.Queries.SearchAgencies
{
    public record SearchAgenciesQuery(string Keyword, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedResult<AgencyDto>>>;
}