using Domain.Common;
using Domain.Enums;

namespace Domain.Events
{
    public record ResponderAssignedToIncidentEvent(Guid IncidentId, Guid ResponderId, ResponderRole Role) : DomainEvent;
}