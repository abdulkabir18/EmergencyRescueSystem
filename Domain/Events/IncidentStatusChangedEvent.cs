using Domain.Common;
using Domain.Enums;

namespace Domain.Events
{
    public class IncidentStatusChangedEvent : DomainEvent
    {
        public Guid IncidentId { get; }
        public IncidentStatus NewStatus { get; }

        public IncidentStatusChangedEvent(Guid incidentId, IncidentStatus newStatus)
        {
            IncidentId = incidentId;
            NewStatus = newStatus;
        }
    }
}
