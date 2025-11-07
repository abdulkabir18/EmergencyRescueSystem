using Application.Common.Dtos;

namespace Application.Features.Agencies.Dtos
{
    public record AddSupportedIncidentRequestModel(Guid AgencyId, IncidentTypeDto TypeDto);
}