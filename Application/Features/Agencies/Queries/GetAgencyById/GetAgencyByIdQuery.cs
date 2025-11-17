using Application.Common.Dtos;
using Application.Features.Agencies.Dtos;
using MediatR;

namespace Application.Features.Agencies.Queries.GetAgencyById
{
    public record GetAgencyByIdQuery(Guid AgencyId) : IRequest<Result<AgencyDto>>;
}