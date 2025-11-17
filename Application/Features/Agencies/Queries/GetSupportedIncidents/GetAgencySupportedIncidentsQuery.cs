using Application.Common.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Features.Agencies.Queries.GetSupportedIncidents
{
    public record GetAgencySupportedIncidentsQuery(Guid AgencyId) : IRequest<Result<List<IncidentType>>>;
}