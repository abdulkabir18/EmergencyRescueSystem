using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces.Notifications
{
    public interface INotificationService
    {
        Task SendUserNotificationAsync(Guid userId, string title, string message, NotificationType type = NotificationType.System, Guid? targetId = null, string? targetType = null);
        Task BroadcastAsync(IEnumerable<Guid> userIds, string title, string message, NotificationType type = NotificationType.System, Guid? targetId = null, string? targetType = null);
        Task NotifySuperAdminIncidentInvalidAsync(Incident incident);
        Task NotifyUserIncidentUpdateAsync(User user, Incident incident);
        Task NotifyAgencyIncidentAsync(Agency agency, Incident incident);
        Task NotifyNearestRespondersAsync(IEnumerable<Responder> responders, Incident incident);
    }
}