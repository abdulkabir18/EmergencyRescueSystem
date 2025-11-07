using Application.Common.Dtos;

namespace Application.Features.Agencies.Dtos
{
    public record RemoveSupportedIncidentRequestModel(Guid AgencyId, IncidentTypeDto TypeDto);
}