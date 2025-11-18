using Application.Common.Dtos;
using Application.Features.Responders.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Responders.Queries.GetRespondersByIncident
{
    public class GetRespondersByIncidentQueryHandler : IRequestHandler<GetRespondersByIncidentQuery, Result<List<ResponderDto>>>
    {
        private readonly IResponderRepository _responderRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetRespondersByIncidentQueryHandler> _logger;

        public GetRespondersByIncidentQueryHandler(IResponderRepository responderRepository, ICacheService cacheService, ILogger<GetRespondersByIncidentQueryHandler> logger)
        {
            _responderRepository = responderRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<List<ResponderDto>>> Handle(GetRespondersByIncidentQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"responders:incident:{request.IncidentId}";

            try
            {
                var cached = await _cacheService.GetAsync<List<ResponderDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Responders for incident {IncidentId} returned from cache.", request.IncidentId);
                    return Result<List<ResponderDto>>.Success(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache lookup failed for responders of incident {IncidentId}", request.IncidentId);
            }

            var responders = (await _responderRepository.GetRespondersByIncidentAsync(request.IncidentId)).ToList();
            if (!responders.Any())
                return Result<List<ResponderDto>>.Failure("No responders assigned to this incident.");

            var items = responders.Select(r => new ResponderDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserFullName = r.User?.FullName ?? string.Empty,
                Email = r.User?.Email.Value ?? string.Empty,
                ProfilePictureUrl = r.User?.ProfilePictureUrl,
                AgencyId = r.AgencyId,
                AgencyName = r.Agency?.Name,
                Status = r.Status.ToString(),
                Coordinates = r.Coordinates != null ? new GeoLocationDto(r.Coordinates.Latitude, r.Coordinates.Longitude) : null,
                CreatedAt = r.CreatedAt
            }).ToList();

            try
            {
                await _cacheService.SetAsync(cacheKey, items, TimeSpan.FromMinutes(1));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache responders for incident {IncidentId}", request.IncidentId);
            }

            return Result<List<ResponderDto>>.Success(items);
        }
    }
}