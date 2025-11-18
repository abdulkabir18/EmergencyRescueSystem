using Application.Common.Dtos;
using Application.Features.Responders.Dtos;
using MediatR;

namespace Application.Features.Responders.Queries.GetRespondersByAgency
{
    public record GetRespondersByAgencyQuery(Guid AgencyId, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<ResponderDto>>;
}