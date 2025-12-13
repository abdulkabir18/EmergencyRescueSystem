using Application.Common.Dtos;
using Application.Features.Incidents.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Incidents.Queries.GetIncidentById
{
    public class GetIncidentByIdQueryHandler : IRequestHandler<GetIncidentByIdQuery, Result<IncidentDto>>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetIncidentByIdQueryHandler> _logger;

        public GetIncidentByIdQueryHandler(IIncidentRepository incidentRepository, ICacheService cacheService, ILogger<GetIncidentByIdQueryHandler> logger)
        {
            _incidentRepository = incidentRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<IncidentDto>> Handle(GetIncidentByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"incident:{request.IncidentId}";

            var cachedDto = await _cacheService.GetAsync<IncidentDto>(cacheKey);
            if (cachedDto != null)
            {
                _logger.LogInformation("Incident {IncidentId} retrieved from cache.", request.IncidentId);
                return Result<IncidentDto>.Success(cachedDto, "Retrieved from cache");
            }

            var incident = await _incidentRepository.GetByIdWithDetailsAsync(request.IncidentId);
            if (incident == null)
            {
                _logger.LogWarning("Incident {IncidentId} not found.", request.IncidentId);
                return Result<IncidentDto>.Failure($"Incident with ID {request.IncidentId} not found.");
                //return Result<IncidentDto>.Success(new IncidentDto(), $"Incident with ID {request.IncidentId} not found.");
            }

            var dto = new IncidentDto
            {
                Id = incident.Id,
                ReferenceNumber = incident.ReferenceCode,
                Title = incident.Title,
                Type = incident.Type.ToString(),
                Confidence = incident.Confidence,
                Status = incident.Status.ToString(),
                Coordinates = new GeoLocationDto(incident.Coordinates.Latitude, incident.Coordinates.Longitude),
                Address = incident.Address != null ? new AddressDto
                {
                    Street = incident.Address.Street,
                    City = incident.Address.City,
                    State = incident.Address.State,
                    LGA = incident.Address.LGA,
                    Country = incident.Address.Country,
                    PostalCode = incident.Address.PostalCode
                } : null,
                OccurredAt = incident.OccurredAt,
                UserId = incident.UserId,
                UserName = incident.User.FullName,
                UserContact = incident.User.Email.Value,
                Media = new IncidentMediaInfoDto(incident.Media.FileUrl, incident.Media.MediaType.ToString()),
                AssignedResponders = incident.AssignedResponders?.Select(ar => new AssignedResponderDto
                {
                    Id = ar.Id,
                    ResponderId = ar.ResponderId,
                    UserId = ar.Responder?.UserId ?? Guid.Empty,
                    Role = ar.Role.ToString(),
                    ResponderName = ar.Responder?.User?.FullName,
                    AgencyName = ar.Responder?.Agency.Name
                }).ToList() ?? []
            };

            var expiration = TimeSpan.FromMinutes(10);
            try
            {
                await _cacheService.SetAsync(cacheKey, dto, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set cache for incident {IncidentId}", request.IncidentId);
            }

            _logger.LogInformation("Incident {IncidentId} retrieved from DB and cached.", incident.Id);
            return Result<IncidentDto>.Success(dto, "Retrieved from database");
        }
    }
}