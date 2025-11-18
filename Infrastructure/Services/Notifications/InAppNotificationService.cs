using Application.Common.Dtos;
using Application.Common.Interfaces.Notifications;
using Application.Common.Interfaces.Realtime;
using Application.Interfaces.External;
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
        private readonly ICacheService _cacheService;
        private readonly IRealtimeNotifier _realtimeNotifier;
        private readonly ILogger<InAppNotificationService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public InAppNotificationService(INotificationRepository notificationRepository, IUserRepository userRepository, ICacheService cacheService, IRealtimeNotifier realtimeNotifier, ILogger<InAppNotificationService> logger, IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _cacheService = cacheService;
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

            await _cacheService.RemoveAsync(GetUnreadCacheKey(recipientId));
            await _cacheService.RemoveByPrefixAsync($"notifications:{recipientId}");

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

            foreach (var id in recipientIds)
            {
                await _cacheService.RemoveAsync(GetUnreadCacheKey(id));
                await _cacheService.RemoveByPrefixAsync($"notifications:{id}");
            }

            _logger.LogInformation("Broadcast notification sent to {Count} users", recipientIds.Count());
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _notificationRepository.GetAsync(notificationId);
            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found", notificationId);
                throw new KeyNotFoundException("Notification not found.");
            }

            notification.MarkAsRead();
            await _notificationRepository.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync(GetUnreadCacheKey(notification.RecipientId));
            await _cacheService.RemoveByPrefixAsync($"notifications:{notification.RecipientId}");

            _logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
        }

        public async Task<PaginatedResult<NotificationDto>> GetUserNotificationsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {

            bool isExist = await _userRepository.IsUserExistByIdAsync(userId);
            if(isExist)
            {
                string cacheKey = $"notifications:{userId}:page:{pageNumber}:size:{pageSize}";
                var cached = await _cacheService.GetAsync<PaginatedResult<NotificationDto>>(cacheKey);

                if (cached != null)
                    return cached;

                var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, pageNumber, pageSize);
                if(notifications == null || notifications.Data.Count == 0)
                {
                    return PaginatedResult<NotificationDto>.Failure("No notifications found.");
                }

                var notificationDto = notifications.Data!.Select(notification => new NotificationDto
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

                var result = PaginatedResult<NotificationDto>.Success(notificationDto, notifications.TotalCount, pageNumber, pageSize);
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
                return result;
            }
            return PaginatedResult<NotificationDto>.Failure("User did not exist");
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            bool isExist = await _userRepository.IsUserExistByIdAsync(userId);
            if (!isExist)
                return -1;

            string cacheKey = GetUnreadCacheKey(userId);
            var cachedCount = await _cacheService.GetAsync<int?>(cacheKey);

            if (cachedCount.HasValue)
                return cachedCount.Value;

            int count = await _notificationRepository.GetUnreadCountAsync(userId);

            await _cacheService.SetAsync(cacheKey, count, TimeSpan.FromMinutes(5));

            return count;
        }

        private static string GetUnreadCacheKey(Guid userId)
            => $"notifications:{userId}:unread-count";
    }
}