using Application.Common.Interfaces.Notifications;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Incidents.EventHandlers
{
    public class IncidentStatusChangedEventHandler : INotificationHandler<IncidentStatusChangedEvent>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IResponderRepository _responderRepository;
        private readonly INotificationService _notificationService;
        private readonly IAgencyRepository _agencyRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<IncidentStatusChangedEventHandler> _logger;

        public IncidentStatusChangedEventHandler(
            IIncidentRepository incidentRepository,
            IUserRepository userRepository,
            IResponderRepository responderRepository,
            INotificationService notificationService,
            IAgencyRepository agencyRepository,
            IEmailService emailService,
            ILogger<IncidentStatusChangedEventHandler> logger)
        {
            _incidentRepository = incidentRepository;
            _userRepository = userRepository;
            _responderRepository = responderRepository;
            _notificationService = notificationService;
            _agencyRepository = agencyRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(IncidentStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "IncidentStatusChangedEvent received for IncidentId {IncidentId}. New status: {Status}",
                    notification.IncidentId, notification.NewStatus);

                var incident = await _incidentRepository.GetByIdWithDetailsAsync(notification.IncidentId);

                if (incident == null)
                {
                    _logger.LogWarning("Incident {IncidentId} not found", notification.IncidentId);
                    return;
                }

                var reporter = await _userRepository.GetAsync(incident.UserId);

                string statusMessage = notification.NewStatus switch
                {
                    IncidentStatus.InProgress => "🚨 A responder is now handling your incident.",
                    IncidentStatus.Resolved => "✅ Your incident has been resolved.",
                    IncidentStatus.Escalated => "⚠️ Your incident has been escalated.",
                    IncidentStatus.Cancelled => "❌ Your incident has been cancelled.",
                    _ => $"Incident status changed to {notification.NewStatus}."
                };

                if (reporter != null)
                {
                    await _notificationService.SendUserNotificationAsync(
                        reporter.Id,
                        "Incident Status Updated",
                        statusMessage,
                        NotificationType.Info,
                        incident.Id,
                        nameof(incident));

                    try
                    {
                            await _emailService.SendEmailAsync(
                            reporter.Email.Value,
                            "Incident Status Updated",
                            statusMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email notification to user {UserId} for IncidentId {IncidentId}",
                            reporter.Id, notification.IncidentId);
                    }
                }

                if(notification.NewStatus == IncidentStatus.InProgress)
                {
                    var responders = incident.AssignedResponders;
                    var responderIds = responders.Select(r => r.Responder.UserId).Distinct();
                    await _notificationService.BroadcastAsync(
                        responderIds,
                        "Incident In Progress",
                        $"The incident '{incident.Title}' is now in progress and requires your attention.",
                        NotificationType.Alert, 
                        incident.Id,
                        nameof(incident)
                        );

                    try
                    {
                        await _emailService.SendEmailAsync(
                            responders.Select(r => r.Responder.User.Email.Value),
                            "Incident In Progress",
                            $"The incident '{incident.Title}' is now in progress and requires your attention.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email notifications to responders for IncidentId {IncidentId}",
                            notification.IncidentId);
                    }
                }
                //else if(notification.NewStatus == IncidentStatus.Escalated)
                //{
                //    var superAdmin = await _userRepository.GetSuperAdminId();
                //    if(superAdmin != null)
                //    {
                //        await _notificationService.SendUserNotificationAsync(
                //            superAdmin.Id,
                //            "Incident Escalated",
                //            $"The incident '{incident.Title}' has been escalated.",
                //            NotificationType.Alert);
                //        try
                //        {
                //            await _emailService.SendEmailAsync(
                //                superAdmin.Email.Value,
                //                "Incident Escalated",
                //                $"The incident '{incident.Title}' has been escalated.");
                //        }
                //        catch (Exception ex)
                //        {
                //            _logger.LogError(ex, "Failed to send email notification to Super Admin for IncidentId {IncidentId}",
                //                notification.IncidentId);
                //        }
                //    }
                //}
                else if (notification.NewStatus == IncidentStatus.Escalated)
                {
                    var title = "Incident Escalated";
                    var body = $"The incident '{incident.Title}' has been escalated and requires urgent attention.";

                    var superAdmin = await _userRepository.GetSuperAdminId();
                    if (superAdmin != null)
                    {
                        await _notificationService.SendUserNotificationAsync(superAdmin.Id, title, body, NotificationType.Alert, incident.Id, nameof(incident));

                        try
                        {
                            await _emailService.SendEmailAsync(superAdmin.Email.Value, title, body);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed sending escalation email to SuperAdmin for IncidentId {IncidentId}",
                                incident.Id);
                        }
                    }

                    var agencies = await _agencyRepository.GetAgenciesBySupportedIncidentAsync(incident.Type);
                    if (agencies == null || agencies.Count() == 0)
                    {
                        _logger.LogWarning("Escalation: No agencies available for IncidentType {Type}", incident.Type);
                        return;
                    }

                    var agencyAdminIds = agencies
                        .Select(a => a.AgencyAdminId)
                        .Where(id => id != Guid.Empty)
                        .Distinct()
                        .ToList();

                    if (agencyAdminIds.Any())
                    {
                        await _notificationService.BroadcastAsync(agencyAdminIds, title, body, NotificationType.Alert, incident.Id, nameof(incident));

                        var allResponders = agencies.SelectMany(a => a.Responders ?? Enumerable.Empty<Responder>()).Where(r => r != null).ToList();
                        
                        if (allResponders.Any())
                        {
                            await _notificationService.BroadcastAsync(allResponders.Select(r => r.UserId), title, body, NotificationType.Alert, incident.Id, nameof(incident));

                            try
                            {
                                await _emailService.SendEmailAsync(allResponders.Select(r => r.User.Email.Value), title, body);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed sending escalation email to Responders for IncidentId {IncidentId}",
                                    incident.Id);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Escalation: No Responders found for IncidentId {IncidentId}", incident.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Escalation: No AgencyAdmins found for IncidentId {IncidentId}", incident.Id);
                    }
                    _logger.LogInformation(
                        "Escalation notifications sent to SuperAdmin, AgencyAdmins and Responders for IncidentId {IncidentId}",
                        incident.Id);
                }
                else if(notification.NewStatus == IncidentStatus.Resolved || notification.NewStatus == IncidentStatus.Cancelled)
                {
                    var responders = incident.AssignedResponders;
                    var responderIds = responders.Select(r => r.Responder.UserId).Distinct();
                    await _notificationService.BroadcastAsync(
                        responderIds,
                        "Incident Update",
                        $"The incident '{incident.Title}' has been {notification.NewStatus.ToString().ToLower()}.",
                        NotificationType.Info,
                        incident.Id,
                        nameof(incident));
                    try
                    {
                        await _emailService.SendEmailAsync(
                            responders.Select(r => r.Responder.User.Email.Value),
                            "Incident Update",
                            $"The incident '{incident.Title}' has been {notification.NewStatus.ToString().ToLower()}.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email notifications to responders for IncidentId {IncidentId}",
                            notification.IncidentId);
                    }

                    foreach (var assigned in responders)
                    {
                        assigned.Responder.UpdateResponderStatus(ResponderStatus.Available);
                        await _responderRepository.UpdateAsync(assigned.Responder);
                    }
                }





                _logger.LogInformation(
                    "Status change notifications sent for IncidentId {IncidentId}",
                    notification.IncidentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error handling IncidentStatusChangedEvent for IncidentId {IncidentId}",
                    notification.IncidentId);
            }
        }
    }
}
