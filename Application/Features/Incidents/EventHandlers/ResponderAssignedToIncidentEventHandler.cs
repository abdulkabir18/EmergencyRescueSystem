using Application.Common.Interfaces.Notifications;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Incidents.EventHandlers
{
    public class ResponderAssignedToIncidentEventHandler : INotificationHandler<ResponderAssignedToIncidentEvent>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IResponderRepository _responderRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ResponderAssignedToIncidentEventHandler> _logger;

        public ResponderAssignedToIncidentEventHandler(IIncidentRepository incidentRepository, IUserRepository userRepository, IResponderRepository responderRepository, INotificationService notificationService, ILogger<ResponderAssignedToIncidentEventHandler> logger)
        {
            _incidentRepository = incidentRepository;
            _userRepository = userRepository;
            _responderRepository = responderRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(ResponderAssignedToIncidentEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("ResponderAssignedToIncidentEvent received for IncidentId: {IncidentId}", notification.IncidentId);

                var incident = await _incidentRepository.GetByIdWithDetailsAsync(notification.IncidentId);
                var responder = await _responderRepository.GetResponderWithDetailsAsync(notification.ResponderId);

                if (incident == null || responder == null)
                {
                    _logger.LogWarning("Incident or Responder not found for event: {IncidentId}, {ResponderId}", notification.IncidentId, notification.ResponderId);
                    return;
                }

                var reporter = await _userRepository.GetAsync(incident.UserId);
                if (reporter != null)
                {
                    await _notificationService.SendUserNotificationAsync(
                        reporter.Id,
                        "🚑 Responder Assigned to Your Incident",
                        $"Responder {responder.User.FullName} ({notification.Role}) has been assigned to your reported incident '{incident.Title}'.",
                        NotificationType.Alert);
                }

                await _notificationService.SendUserNotificationAsync(
                    responder.UserId,
                    "✅ Incident Assignment",
                    $"You have been assigned to incident '{incident.Title}' as {notification.Role}.",
                    NotificationType.Alert);

                _logger.LogInformation("Responder {ResponderId} assigned to incident {IncidentId} as {Role}",
                notification.ResponderId, notification.IncidentId, notification.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ResponderAssignedToIncidentEvent for IncidentId: {IncidentId}", notification.IncidentId);
            }
        }
    }
}