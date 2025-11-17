using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Dtos;
using Application.Features.Responders.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Responders.Queries.GetNearbyResponders
{
    public class GetNearbyRespondersQueryHandler : IRequestHandler<GetNearbyRespondersQuery, Result<PaginatedResult<ResponderDto>>>
    {
        private readonly IResponderRepository _responderRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetNearbyRespondersQueryHandler> _logger;

        public GetNearbyRespondersQueryHandler(IResponderRepository responderRepository, ICacheService cacheService, ILogger<GetNearbyRespondersQueryHandler> logger)
        {
            _responderRepository = responderRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<PaginatedResult<ResponderDto>>> Handle(GetNearbyRespondersQuery request, CancellationToken cancellationToken)
        {
            var lat = request.Latitude.ToString("F4");
            var lon = request.Longitude.ToString("F4");
            var cacheKey = $"responders:nearby:lat:{lat}:lon:{lon}:r:{request.RadiusKm}:p{request.PageNumber}:s{request.PageSize}";

            try
            {
                var cached = await _cacheService.GetAsync<PaginatedResult<ResponderDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Nearby responders for {Lat},{Lon} returned from cache.", lat, lon);
                    return Result<PaginatedResult<ResponderDto>>.Success(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache lookup failed for nearby responders at {Lat},{Lon}", lat, lon);
            }

            var paged = await _responderRepository.GetNearbyRespondersAsync(request.Latitude, request.Longitude, request.RadiusKm, request.PageNumber, request.PageSize);

            if (paged == null || paged.Data == null || !paged.Data.Any())
                return Result<PaginatedResult<ResponderDto>>.Failure("No nearby responders found.");

            var items = paged.Data.Select(r => new ResponderDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserFullName = r.User?.FullName ?? string.Empty,
                Email = r.User?.Email.Value ?? string.Empty,
                ProfilePictureUrl = r.User?.ProfilePictureUrl,
                AgencyId = r.AgencyId,
                AgencyName = r.Agency?.Name,
                Status = r.Status.ToString(),
                Coordinates = r.Coordinates != null ? new Application.Common.Dtos.GeoLocationDto(r.Coordinates.Latitude, r.Coordinates.Longitude) : null,
                CreatedAt = r.CreatedAt
            }).ToList();

            var resultPage = PaginatedResult<ResponderDto>.Create(items, paged.TotalCount, request.PageNumber, request.PageSize);

            try
            {
                await _cacheService.SetAsync(cacheKey, resultPage, TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache nearby responders for {Lat},{Lon}", lat, lon);
            }

            return Result<PaginatedResult<ResponderDto>>.Success(resultPage);
        }
    }
}