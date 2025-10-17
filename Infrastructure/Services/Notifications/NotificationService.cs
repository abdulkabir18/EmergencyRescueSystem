using Application.Common.Interfaces.Notifications;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IInAppNotificationService _inAppNotificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUserRepository userRepository, IEmailService emailService, IInAppNotificationService inAppNotificationService, IUnitOfWork unitOfWork, ILogger<NotificationService> logger)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _inAppNotificationService = inAppNotificationService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        //public async Task NotifyEmergencyContactsAsync(Guid userId, Incident incident)
        //{
        //    var contactEmails = await _userRepository.GetEmergencyContactEmailsAsync(userId);
        //    if (!contactEmails.Any())
        //    {
        //        _logger.LogWarning("No emergency contacts found for user {UserId}", userId);
        //        return;
        //    }

        //    var subject = "🚨 Emergency Alert from NaijaRescue";
        //    var body = $@"
        //        <h3>Emergency Alert!</h3>
        //        <p><strong>Incident Type:</strong> {incident.Type}</p>
        //        <p><strong>Location:</strong> {incident.Location}</p>
        //        <p><strong>Time:</strong> {incident.CreatedAt}</p>
        //        <p>Please reach out or take necessary action immediately.</p>
        //    ";

        //    await _emailService.SendEmailAsync(contactEmails, subject, body);

        //    await _unitOfWork.SaveChangesAsync();

        //    _logger.LogInformation("Emergency contacts notified for incident {IncidentId}", incident.Id);
        //}

        public async Task SendUserNotificationAsync(Guid userId, string title, string message)
        {
            await _inAppNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                NotificationType.System
            );
        }

        public async Task BroadcastAsync(IEnumerable<Guid> userIds, string title, string message)
        {
            await _inAppNotificationService.BroadcastAsync(
                userIds,
                title,
                message,
                NotificationType.System
            );

            _logger.LogInformation("Broadcast notification sent | Title: {Title} | Recipients: {Count}", title, userIds.Count());
        }
    }
}
