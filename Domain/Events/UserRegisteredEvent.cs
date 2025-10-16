using Domain.Common;
using Domain.Enums;

namespace Domain.Events
{
    public class UserRegisteredEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string FullName { get; }
        public string Email { get; }
        public UserRole Role { get; }

        public UserRegisteredEvent(Guid userId, string fullName, string email, UserRole role)
        {
            UserId = userId;
            FullName = fullName;
            Email = email;
            Role = role;
        }
    }
}
