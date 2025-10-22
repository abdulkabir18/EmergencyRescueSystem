using Domain.Common;
using Domain.Enums;

namespace Domain.Events
{
    public class ResponderStatusUpdatedEvent : DomainEvent
    {
        public Guid ResponderId { get; }
        public ResponderStatus NewStatus { get; }

        public ResponderStatusUpdatedEvent(Guid responderId, ResponderStatus newStatus)
        {
            ResponderId = responderId;
            NewStatus = newStatus;
        }
    }
}
