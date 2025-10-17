using Domain.Entities;

namespace Application.Common.Interfaces.Notifications
{
    public interface INotificationService
    {
        //Task NotifyEmergencyContactsAsync(Guid userId, Incident incident);

        Task SendUserNotificationAsync(Guid userId, string title, string message);

        Task BroadcastAsync(IEnumerable<Guid> userIds, string title, string message);
    }
}
