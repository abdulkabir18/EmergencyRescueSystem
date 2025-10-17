using Domain.Common;
using Domain.Common.Exceptions;
using Domain.Enums;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Responder : AuditableEntity
    {
        public Guid UserId { get; private set; }
        public User User { get; private set; } = default!;
        public Guid AgencyId { get; private set; }
        public Agency Agency { get; private set; } = default!;
        public ResponderStatus Status { get; private set; }
        public GeoLocation? AssignedLocation { get; private set; }

        //public ICollection<IncidentResponder> IncidentAssignments { get; private set; } = [];

        private Responder() { }

        public Responder(Guid userId, Guid agencyId)
        {
            if (userId == Guid.Empty) throw new ValidationException("UserId is required.");
            if (agencyId == Guid.Empty) throw new ValidationException("AgencyId is required.");

            UserId = userId;
            AgencyId = agencyId;
            Status = ResponderStatus.Unreachable;
        }

        public void UpdateResponderStatus(ResponderStatus status)
        {
            Status = status;

            //AddDomainEvent(new ResponderStatusUpdatedEvent(Id, status));
        }

        public void AssignLocation(GeoLocation location)
        {
            AssignedLocation = location;
            // AddDomainEvent(new ResponderAssignedLocationEvent(Id, location));
        }
    }
}