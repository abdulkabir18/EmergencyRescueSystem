using Domain.Common;
using Domain.Enums;

namespace Domain.Events
{
    public record UserRegisteredEvent(Guid UserId, string FullName, string Email, UserRole Role) : DomainEvent;
}
