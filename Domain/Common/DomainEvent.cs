using MediatR;

namespace Domain.Common
{
    public abstract class DomainEvent : IDomainEvent , INotification
    {
        public DateTime OccurredOn { get; protected set; }

        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }
    }
}