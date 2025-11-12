using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Incidents.EventHandlers
{
    public class IncidentCreatedEventHandler : INotificationHandler<IncidentCreatedEvent>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IAIService _aiService;
        private readonly IGeocodingService _geocodingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IncidentCreatedEventHandler> _logger;

        public IncidentCreatedEventHandler(IIncidentRepository incidentRepository, IAIService aiService, IGeocodingService geocodingService, IUnitOfWork unitOfWork, ILogger<IncidentCreatedEventHandler> logger)
        {
            _incidentRepository = incidentRepository;
            _aiService = aiService;
            _geocodingService = geocodingService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(IncidentCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var incident = await _incidentRepository.GetByIdWithDetailsAsync(notification.IncidentId);
                if (incident == null)
                {
                    _logger.LogWarning("Incident not found for IncidentCreatedEvent: {IncidentId}", notification.IncidentId);
                    return;
                }

                var media = incident.Medias?.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(media?.FileUrl))
                {
                    try
                    {
                        var aiResult = await _aiService.AnalyzeIncidentMediaAsync(media.FileUrl);
                        if (aiResult is not null)
                        {
                            incident.ApplyAiAnalysis(aiResult.Title, aiResult.Type, aiResult.Confidence);
                            _logger.LogInformation("Applied AI analysis to incident {IncidentId}: Title={Title}, Type={Type}, Confidence={Confidence}",
                                incident.Id, aiResult.Title, aiResult.Type, aiResult.Confidence);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "AI analysis failed for incident {IncidentId} (media: {MediaUrl})", incident.Id, media.FileUrl);
                    }
                }
                else
                {
                    _logger.LogDebug("No media to analyze for incident {IncidentId}", incident.Id);
                }

                try
                {
                    var address = await _geocodingService.GetAddressFromCoordinatesAsync(incident.Coordinates.Latitude, incident.Coordinates.Longitude);
                    if (address != null)
                    {
                        incident.SetAddress(new Address(
                            address.Street ?? string.Empty,
                            address.City ?? string.Empty,
                            address.State ?? string.Empty,
                            address.LGA ?? string.Empty,
                            address.Country ?? string.Empty,
                            address.PostalCode ?? string.Empty
                        ));
                        _logger.LogInformation("Reverse geocoded address for incident {IncidentId}", incident.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Geocoding failed for incident {IncidentId}", incident.Id);
                }

                await _incidentRepository.UpdateAsync(incident);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("IncidentCreatedEvent handled successfully for {IncidentId}", incident.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error while processing IncidentCreatedEvent for {IncidentId}", notification.IncidentId);
            }
        }
    }
}