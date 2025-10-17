using Application.Common.Dtos;
using Application.Common.Interfaces.Notifications;
using Application.Common.Interfaces.Realtime;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Notifications
{
    public class InAppNotificationService : IInAppNotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRealtimeNotifier _realtimeNotifier;
        private readonly ILogger<InAppNotificationService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public InAppNotificationService(INotificationRepository notificationRepository, IUserRepository userRepository, IRealtimeNotifier realtimeNotifier, ILogger<InAppNotificationService> logger, IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _realtimeNotifier = realtimeNotifier;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task SendToUserAsync(Guid recipientId, string title, string message, NotificationType type, Guid? targetId = null, string? targetType = null)
        {
            var notification = new Notification(recipientId, title, message, type, targetId, targetType);

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            await _realtimeNotifier.SendToUserAsync(recipientId, "ReceiveNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.CreatedAt
            });

            _logger.LogInformation("Notification saved and sent to user {UserId}", recipientId);
        }

        public async Task BroadcastAsync(IEnumerable<Guid> recipientIds, string title, string message,
            NotificationType type, Guid? targetId = null, string? targetType = null)
        {
            var notifications = recipientIds.Select(id => new Notification(id, title, message, type, targetId, targetType)).ToList();

            await _notificationRepository.AddAsync(notifications);
            await _unitOfWork.SaveChangesAsync();

            await _realtimeNotifier.SendToUsersAsync(recipientIds, "ReceiveNotification", new
            {
                Title = title,
                Message = message,
                Type = type.ToString(),
                TargetId = targetId,
                TargetType = targetType
            });

            _logger.LogInformation("Broadcast notification sent to {Count} users", recipientIds.Count());
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _notificationRepository.GetAsync(notificationId);
            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found", notificationId);
                return;
            }

            notification.MarkAsRead();
            await _notificationRepository.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
        }

        public async Task<PaginatedResult<NotificationDto>> GetUserNotificationsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            bool isExist = await _userRepository.IsUserExistByIdAsync(userId);
            if(isExist)
            {
                var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, pageNumber, pageSize);

                var notificationDto = notifications!.Data.Select(notification => new NotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    TargetId = notification.TargetId,
                    TargetType = notification.TargetType,
                    Type = notification.Type
                }).ToList();

                var result = PaginatedResult<NotificationDto>.Create(notificationDto, notifications.TotalCount, pageNumber, pageSize);
                return result;
            }
            return PaginatedResult<NotificationDto>.Failure("User did not exist");
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            bool isExist = await _userRepository.IsUserExistByIdAsync(userId);
            if (!isExist)
                return -1;

            int count = await _notificationRepository.GetUnreadCountAsync(userId);
            return count;
        }
    }
}