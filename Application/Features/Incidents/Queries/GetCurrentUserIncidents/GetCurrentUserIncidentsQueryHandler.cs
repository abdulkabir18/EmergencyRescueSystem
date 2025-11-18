using Application.Common.Dtos;
using Application.Features.Incidents.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Incidents.Queries.GetCurrentUserIncidents
{
    public class GetCurrentUserIncidentsQueryHandler : IRequestHandler<GetCurrentUserIncidentsQuery, PaginatedResult<IncidentDto>>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetCurrentUserIncidentsQueryHandler> _logger;

        public GetCurrentUserIncidentsQueryHandler(
            IIncidentRepository incidentRepository,
            ICurrentUserService currentUserService,
            ICacheService cacheService,
            ILogger<GetCurrentUserIncidentsQueryHandler> logger)
        {
            _incidentRepository = incidentRepository;
            _currentUserService = currentUserService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PaginatedResult<IncidentDto>> Handle(GetCurrentUserIncidentsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized request to get current user's incidents.");
                return PaginatedResult<IncidentDto>.Failure("Unauthorized.");
            }

            var cacheKey = $"incidents:user:{userId}:page:{request.PageNumber}:size:{request.PageSize}";
            var cached = await _cacheService.GetAsync<PaginatedResult<IncidentDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("User {UserId} incidents page {Page} returned from cache.", userId, request.PageNumber);
                return cached;
            }

            var incidentsPaged = await _incidentRepository.GetIncidentsByUserAsync(userId, request.PageNumber, request.PageSize);

            if (incidentsPaged == null || incidentsPaged.Data == null || !incidentsPaged.Data.Any())
            {
                _logger.LogInformation("No incidents found for user {UserId} page {Page}", userId, request.PageNumber);

                return PaginatedResult<IncidentDto>.Failure("No incidents found.");
            }

            var items = incidentsPaged.Data.Select(i => new IncidentDto
            {
                Id = i.Id,
                Title = i.Title,
                Type = i.Type.ToString(),
                Confidence = i.Confidence,
                Status = i.Status.ToString(),
                Coordinates = new GeoLocationDto(i.Coordinates.Latitude, i.Coordinates.Longitude),
                Address = i.Address != null ? new AddressDto
                {
                    Street = i.Address.Street,
                    City = i.Address.City,
                    State = i.Address.State,
                    LGA = i.Address.LGA,
                    Country = i.Address.Country,
                    PostalCode = i.Address.PostalCode
                } : null,
                OccurredAt = i.OccurredAt,
                UserId = i.UserId,
                Media = i.Medias?.Select(m => new IncidentMediaInfoDto(m.FileUrl, m.MediaType.ToString())).ToList() ?? new List<IncidentMediaInfoDto>(),
                AssignedResponders = i.AssignedResponders?.Select(ar => new AssignedResponderDto
                {
                    Id = ar.Id,
                    ResponderId = ar.ResponderId,
                    UserId = ar.Responder?.UserId ?? Guid.Empty,
                    Role = ar.Role.ToString(),
                    ResponderName = ar.Responder?.User?.FullName
                }).ToList() ?? new List<AssignedResponderDto>()
            }).ToList();

            var resultPage = PaginatedResult<IncidentDto>.Success(items, incidentsPaged.TotalCount, request.PageNumber, request.PageSize);

            try
            {
                await _cacheService.SetAsync(cacheKey, resultPage, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache user {UserId} incidents page {Page}", userId, request.PageNumber);
            }

            _logger.LogInformation("Returned {Count} incidents for user {UserId} (page {Page})", items.Count, userId, request.PageNumber);
            return resultPage;
        }
    }
}