using Application.Common.Dtos;
using Application.Features.Responders.Dtos;
using MediatR;

namespace Application.Features.Responders.Queries.GetNearbyResponders
{
    public record GetNearbyRespondersQuery(double Latitude, double Longitude, double RadiusKm, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<ResponderDto>>;
}