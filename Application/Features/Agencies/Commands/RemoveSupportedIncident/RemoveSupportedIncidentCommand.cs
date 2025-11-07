using Application.Common.Dtos;
using Application.Features.Agencies.Dtos;
using MediatR;

namespace Application.Features.Agencies.Commands.RemoveSupportedIncident
{
    public record RemoveSupportedIncidentCommand(RemoveSupportedIncidentRequestModel Model) : IRequest<Result<Guid>>;
}