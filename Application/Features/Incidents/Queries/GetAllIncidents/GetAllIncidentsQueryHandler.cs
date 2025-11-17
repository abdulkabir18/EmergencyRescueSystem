using Application.Common.Dtos;
using Application.Features.Incidents.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Incidents.Queries.GetAllIncidents
{
    public class GetAllIncidentsQueryHandler : IRequestHandler<GetAllIncidentsQuery, Result<PaginatedResult<IncidentDto>>>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetAllIncidentsQueryHandler> _logger;

        public GetAllIncidentsQueryHandler(IIncidentRepository incidentRepository, ICacheService cacheService, ILogger<GetAllIncidentsQueryHandler> logger)
        {
            _incidentRepository = incidentRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<PaginatedResult<IncidentDto>>> Handle(GetAllIncidentsQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"incidents:page:{request.PageNumber}:size:{request.PageSize}";

            var cached = await _cacheService.GetAsync<PaginatedResult<IncidentDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Incidents page {Page} retrieved from cache.", request.PageNumber);
                return Result<PaginatedResult<IncidentDto>>.Success(cached);
            }

            var incidentsPaged = await _incidentRepository.GetAllIncidentsAsync(request.PageNumber, request.PageSize);

            if (incidentsPaged == null || incidentsPaged.Data == null || !incidentsPaged.Data.Any())
            {
                _logger.LogInformation("No incidents found for page {Page}, size {Size}", request.PageNumber, request.PageSize);
                return Result<PaginatedResult<IncidentDto>>.Failure("No incidents found.");
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
                Media = i.Medias?.Select(m => new IncidentMediaInfoDto(m.FileUrl, m.MediaType.ToString())).ToList() ?? [],
                AssignedResponders = i.AssignedResponders?.Select(ar => new AssignedResponderDto
                {
                    Id = ar.Id,
                    ResponderId = ar.ResponderId,
                    UserId = ar.Responder?.UserId ?? Guid.Empty,
                    Role = ar.Role.ToString(),
                    ResponderName = ar.Responder?.User?.FullName
                }).ToList() ?? []
            }).ToList();

            var resultPage = PaginatedResult<IncidentDto>.Create(items, incidentsPaged.TotalCount, request.PageNumber, request.PageSize);

            try
            {
                await _cacheService.SetAsync(cacheKey, resultPage, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache incidents page {Page}", request.PageNumber);
            }

            _logger.LogInformation("Retrieved {Count} incidents (page {Page}) from DB and cached.", items.Count, request.PageNumber);
            return Result<PaginatedResult<IncidentDto>>.Success(resultPage);
        }
    }
}