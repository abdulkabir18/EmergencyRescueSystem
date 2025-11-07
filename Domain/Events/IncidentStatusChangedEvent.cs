using Domain.Common;
using Domain.Enums;

namespace Domain.Events
{
    public record IncidentStatusChangedEvent(Guid IncidentId, IncidentStatus NewStatus) : DomainEvent;
}
