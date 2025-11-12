using Domain.Common;
using Domain.Enums;

namespace Domain.Events
{
    public record IncidentAnalyzedEvent(Guid IncidentId, IncidentType Type, double Confidence, bool IsValid) : DomainEvent;
}
