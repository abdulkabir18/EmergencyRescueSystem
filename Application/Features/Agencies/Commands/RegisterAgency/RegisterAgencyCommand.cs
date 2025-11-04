using Application.Common.Dtos;
using Application.Features.Agencies.Dtos;
using MediatR;

namespace Application.Features.Agencies.Commands.RegisterAgency
{
    public record RegisterAgencyCommand(RegisterAgencyRequestModel Model, IncidentTypesDto IncidentTypes) : IRequest<Result<Guid>>;
}