using Domain.Common;
using Domain.Enums;

namespace Domain.Events
{
    public class ResponderAssignedToIncidentEvent : DomainEvent
    {
        public Guid IncidentId { get; }
        public Guid ResponderId { get; }
        public ResponderRole Role { get; }

        public ResponderAssignedToIncidentEvent(Guid incidentId, Guid responderId, ResponderRole role)
        {
            IncidentId = incidentId;
            ResponderId = responderId;
            Role = role;
        }
    }
}