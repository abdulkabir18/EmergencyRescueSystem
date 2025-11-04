using Domain.Enums;

namespace Application.Common.Dtos
{
    public record IncidentTypeDto
    {
        public IncidentType AcceptedIncidentType { get; set; }
    }
}
