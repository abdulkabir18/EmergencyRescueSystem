using Domain.Common;
using Domain.Enums;

namespace Domain.Events
{
    public record ResponderStatusUpdatedEvent(Guid ResponderId, ResponderStatus NewStatus) : DomainEvent;
}