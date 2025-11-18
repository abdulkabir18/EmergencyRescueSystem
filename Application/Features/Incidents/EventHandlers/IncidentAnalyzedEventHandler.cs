using Application.Common.Interfaces.Notifications;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Application.Features.Incidents.EventHandlers
{
    public class IncidentAnalyzedEventHandler : INotificationHandler<IncidentAnalyzedEvent>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAgencyRepository _agencyRepository;
        private readonly IResponderRepository _responderRepository;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<IncidentAnalyzedEventHandler> _logger;

        public IncidentAnalyzedEventHandler(IIncidentRepository incidentRepository, IUserRepository userRepository, IAgencyRepository agencyRepository, IResponderRepository responderRepository, IEmailService emailService, INotificationService notificationService, ILogger<IncidentAnalyzedEventHandler> logger)
        {
            _incidentRepository = incidentRepository;
            _userRepository = userRepository;
            _agencyRepository = agencyRepository;
            _responderRepository = responderRepository;
            _emailService = emailService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(IncidentAnalyzedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var incident = await _incidentRepository.GetByIdWithDetailsAsync(notification.IncidentId);
                if (incident == null)
                {
                    _logger.LogWarning("Incident with ID {IncidentId} not found.", notification.IncidentId);
                    return;
                }

                var reporter = await _userRepository.GetAsync(incident.UserId);
                if (reporter == null)
                {
                    _logger.LogWarning("Reporter for incident {IncidentId} not found.", incident.Id);
                    return;
                }

                if (incident.Status == IncidentStatus.Invalid || notification.IsValid == false)
                {
                    await _notificationService.NotifySuperAdminIncidentInvalidAsync(incident);
                    await _notificationService.NotifyUserIncidentUpdateAsync(reporter, incident);

                    _logger.LogInformation("Invalid incident {IncidentId} notifications dispatched.", incident.Id);
                    return;
                }

                var userTitle = "✅ Incident Analyzed Successfully";
                var userMessage = $"Your reported incident '{incident.Title}' has been classified as {incident.Type}.";
                await _notificationService.SendUserNotificationAsync(reporter.Id, userTitle, userMessage, NotificationType.Info, incident.Id, nameof(incident));
                try
                {
                    await _emailService.SendEmailAsync(reporter.Email.Value, userTitle, $"<p>{userMessage}</p>");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to email reporter {UserId} for incident {IncidentId}", reporter.Id, incident.Id);
                }

                var agencies = await _agencyRepository.GetAgenciesBySupportedIncidentAsync(notification.Type);
                if (agencies == null || agencies.Count() == 0)
                {
                    _logger.LogWarning("No agencies found for type {Type}.", notification.Type);

                    await _notificationService.NotifySuperAdminIncidentInvalidAsync(incident);

                    var pendingTitle = "⚠️ Incident Pending Manual Assignment";
                    var pendingMessage =
                        $"Your reported incident '{incident.Title}' has been analyzed as '{incident.Type}', " +
                        "but currently no agency is available to handle it. Our team has been alerted.";

                    await _notificationService.SendUserNotificationAsync(reporter.Id, pendingTitle, pendingMessage, NotificationType.System, incident.Id, nameof(incident));
                    try
                    {
                        await _emailService.SendEmailAsync(reporter.Email.Value, pendingTitle, pendingMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to email reporter {UserId} for unhandled incident {IncidentId}", reporter.Id, incident.Id);
                    }

                    _logger.LogInformation("SuperAdmin and reporter notified for unhandled incident {IncidentId}", incident.Id);
                    return;
                }

                var allResponders = agencies.SelectMany(a => a.Responders ?? Enumerable.Empty<Responder>()).Where(r => r != null).ToList();
                if (allResponders.Count == 0)
                {
                    _logger.LogWarning("No responders found for incident type {Type}.", notification.Type);

                    await _notificationService.NotifySuperAdminIncidentInvalidAsync(incident);
                    return;
                }

                var nearestResponders = FindNearestResponders(allResponders, incident.Coordinates, 3);
                if (nearestResponders == null || nearestResponders.Count == 0)
                {
                    _logger.LogWarning("No suitable responders found for incident {IncidentId}.", incident.Id);

                    await _notificationService.NotifySuperAdminIncidentInvalidAsync(incident);
                    return;
                }

                foreach (var agency in agencies)
                {
                    try
                    {
                        await _notificationService.NotifyAgencyIncidentAsync(agency, incident);

                        if (agency.Email != null && !string.IsNullOrWhiteSpace(agency.Email.Value))
                        {
                            var subject = $"New Incident: {incident.Title ?? "Unspecified"}";
                            var body = new StringBuilder();
                            body.AppendLine($"<p>Incident Type: <b>{incident.Type}</b></p>");
                            body.AppendLine($"<p>Occurred At: {incident.OccurredAt:u}</p>");
                            if (incident.Address != null)
                                body.AppendLine($"<p>Location: {incident.Address}</p>");
                            await _emailService.SendEmailAsync(agency.Email.Value, subject, body.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to notify agency {AgencyId} for incident {IncidentId}", agency.Id, incident.Id);
                    }
                }

                try
                {
                    await _notificationService.NotifyNearestRespondersAsync(nearestResponders, incident);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to notify nearest responders for incident {IncidentId}", incident.Id);
                }

                _logger.LogInformation("Incident {IncidentId} assigned to nearest responders.", incident.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling IncidentAnalyzedEvent for Incident ID {IncidentId}", notification.IncidentId);
            }   
        }

        private static List<Responder> FindNearestResponders(IEnumerable<Responder> responders, GeoLocation incidentLocation, int maxResults = 3)
        {
            if (incidentLocation == null) return new List<Responder>();

            var respondersWithDistance = responders
                .Where(r => r.Coordinates != null)
                .Select(r => new
                {
                    Responder = r,
                    Distance = CalculateDistance(
                        incidentLocation.Latitude,
                        incidentLocation.Longitude,
                        r.Coordinates.Latitude,
                        r.Coordinates.Longitude)
                })
                .OrderBy(x => x.Distance)
                .Take(maxResults)
                .Select(x => x.Responder)
                .ToList();

            return respondersWithDistance;
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; 
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) *
                    Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Asin(Math.Sqrt(a));
            return R * c;
        }
    }
}
