using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Events
{
    public class IncidentCreatedEvent : DomainEvent
    {
        public Guid IncidentId { get; }
        public Guid? UserId { get; }
        public IncidentType Type { get; }
        public GeoLocation Location { get; }
        public Address? Address { get; }

        public IncidentCreatedEvent(Guid incidentId, Guid? userId, IncidentType type, GeoLocation location, Address? address)
        {
            IncidentId = incidentId;
            UserId = userId;
            Type = type;
            Location = location;
            Address = address;
        }
    }
}
