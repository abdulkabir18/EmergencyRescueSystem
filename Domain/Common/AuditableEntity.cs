namespace Domain.Common
{
    public abstract class AuditableEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string? CreatedBy { get; private set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public bool IsDeleted { get; private set; }

        public void SetCreatedBy(string createdBy)
        {
            if (string.IsNullOrWhiteSpace(createdBy))
                throw new ArgumentException("CreatedBy is required.");
            CreatedBy = createdBy;
        }

        public void MarkUpdated() => UpdatedAt = DateTime.UtcNow;

        public void SoftDelete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }


        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent @event)
            => _domainEvents.Add(@event);

        public void ClearDomainEvents()
            => _domainEvents.Clear();

    }
}
