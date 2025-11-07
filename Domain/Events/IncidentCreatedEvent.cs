using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Events
{
    public record IncidentCreatedEvent(Guid IncidentId, Guid UserId, IncidentType Type, GeoLocation Location) : DomainEvent;
}