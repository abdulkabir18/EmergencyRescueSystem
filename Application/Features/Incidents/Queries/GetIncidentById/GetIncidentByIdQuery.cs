using Application.Common.Dtos;
using Application.Features.Incidents.Dtos;
using MediatR;

namespace Application.Features.Incidents.Queries.GetIncidentById
{
    public record GetIncidentByIdQuery(Guid IncidentId) : IRequest<Result<IncidentDto>>;
}