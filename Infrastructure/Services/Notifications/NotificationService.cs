using Application.Common.Interfaces.Notifications;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Infrastructure.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IInAppNotificationService _inAppNotificationService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUserRepository userRepository, IEmailService emailService, IInAppNotificationService inAppNotificationService, ILogger<NotificationService> logger)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _inAppNotificationService = inAppNotificationService;
            _logger = logger;
        }

        public async Task SendUserNotificationAsync(Guid userId, string title, string message, NotificationType type = NotificationType.System, Guid? targetId = null, string? targetType = null)
        {
            if(userId == Guid.Empty || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message)) 
                throw new ArgumentNullException("Some feild is null or empty");

            await _inAppNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                type,
                targetId,
                targetType
            );
        }

        public async Task BroadcastAsync(IEnumerable<Guid> userIds, string title, string message, NotificationType type = NotificationType.System, Guid? targetId = null, string? targetType = null)
        {
            if (userIds.Count() == 0)
                throw new Exception("No id found");

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
                throw new ArgumentNullException("So feild is empty");

            await _inAppNotificationService.BroadcastAsync(
                userIds,
                title,
                message,
                type,
                targetId,
                targetType
            );

            _logger.LogInformation("Broadcast notification sent | Title: {Title} | Recipients: {Count}", title, userIds.Count());
        }

        public async Task NotifySuperAdminIncidentInvalidAsync(Incident incident)
        {
            var title = "⚠️ Incident Requires Manual Review";
            var message = new StringBuilder();
            message.AppendLine($"Incident ID: {incident.Id}");
            message.AppendLine($"Title: {incident.Title}");
            message.AppendLine($"Type: {incident.Type}");
            message.AppendLine($"Confidence: {incident.Confidence}");
            message.AppendLine($"Location: {incident.Address?.ToFullAddress() ?? "Unknown"}");
            message.AppendLine();
            message.AppendLine("This incident could not be confidently classified by the AI and requires manual review.");

            User superAdmin = await _userRepository.GetSuperAdminId();
            
            try
            {
                await SendUserNotificationAsync(superAdmin.Id, title, message.ToString(), NotificationType.Alert, incident.Id, nameof(incident));
                await _emailService.SendEmailAsync(superAdmin.Email.Value, title, message.ToString());
                _logger.LogInformation("Notified SuperAdmins about invalid incident {IncidentId}", incident.Id);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to notify SuperAdmins about invalid incident {IncidentId}", incident.Id);
            }

        }

        public async Task NotifyUserIncidentUpdateAsync(User user, Incident incident)
        {
            var title = "📢 Your Incident Update";
            var message = $"Your reported incident ({incident.Title}) has been analyzed and classified as {incident.Type}.";

            await SendUserNotificationAsync(user.Id, title, message, NotificationType.Info, incident.Id, nameof(incident));
            await _emailService.SendEmailAsync(user.Email, title, message);
        }

        public async Task NotifyAgencyIncidentAsync(Agency agency, Incident incident)
        {
            var title = $"🚨 New {incident.Type} Incident Reported";
            var message = $"An incident ({incident.Title}) has been confirmed near {incident.Address?.ToFullAddress()}. Please assign responders.";

            await SendUserNotificationAsync(agency.AgencyAdminId, title, message, NotificationType.Alert, incident.Id, nameof(incident));
            await _emailService.SendEmailAsync(agency.Email, title, message);
        }

        public async Task NotifyNearestRespondersAsync(IEnumerable<Responder> responders, Incident incident)
        {
            var title = $"🚨 Nearby {incident.Type} Incident Alert";
            var message = $"You are close to a {incident.Type} incident at {incident.Address?.ToFullAddress()}. Please standby or respond.";

            await BroadcastAsync(responders.Select(r => r.UserId), title, message, NotificationType.Broadcast, incident.Id, nameof(incident));
            await _emailService.SendEmailAsync(responders.Select(r => r.User.Email.Value), title, message);

            _logger.LogInformation("Notified {Count} responders for incident {IncidentId}", responders.Count(), incident.Id);
        }
    }
}