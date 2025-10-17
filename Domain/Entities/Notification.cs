using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Notification : AuditableEntity
    {
        public Guid RecipientId { get; private set; }
        public User Recipient { get; private set; } = default!;

        public string Title { get; private set; } = default!;
        public string Message { get; private set; } = default!;
        public NotificationType Type { get; private set; } = default!;
        public bool IsRead { get; private set; }
        public Guid? TargetId { get; private set; }
        public string? TargetType { get; private set; }

        private Notification() { }

        public Notification(Guid recipientId, string title, string message, NotificationType type, Guid? targetId = null, string? targetType = null)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.");
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be empty.");

            RecipientId = recipientId;
            Title = title;
            Message = message;
            Type = type;
            TargetId = targetId;
            TargetType = targetType;
            IsRead = false;
        }

        public void MarkAsRead()
        {
            if (!IsRead)
                IsRead = true;
        }
    }
}