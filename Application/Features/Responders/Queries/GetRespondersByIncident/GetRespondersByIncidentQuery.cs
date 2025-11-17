using Application.Common.Dtos;
using Application.Features.Responders.Dtos;
using MediatR;

namespace Application.Features.Responders.Queries.GetRespondersByIncident
{
    public record GetRespondersByIncidentQuery(Guid IncidentId) : IRequest<Result<List<ResponderDto>>>;
}