using Application.Common.Dtos;
using Application.Features.Agencies.Dtos;
using MediatR;

namespace Application.Features.Agencies.Commands.AddSupportedIncident
{
    public record AddSupportedIncidentCommand(AddSupportedIncidentRequestModel Model) : IRequest<Result<Unit>>;
}