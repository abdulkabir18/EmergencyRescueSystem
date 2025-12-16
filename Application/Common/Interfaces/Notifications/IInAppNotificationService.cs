using Application.Common.Dtos;
using Domain.Enums;

namespace Application.Common.Interfaces.Notifications
{
    public interface IInAppNotificationService
    {
        Task SendToUserAsync(Guid recipientId, string title, string message, NotificationType type, Guid? targetId = null, string? targetType = null);
        Task BroadcastAsync(IEnumerable<Guid> recipientIds, string title, string message, NotificationType type, Guid? targetId = null, string? targetType = null);
        Task MarkAsReadAsync(Guid notificationId);
        Task<PaginatedResult<NotificationDto>> GetUserNotificationsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
    }
}