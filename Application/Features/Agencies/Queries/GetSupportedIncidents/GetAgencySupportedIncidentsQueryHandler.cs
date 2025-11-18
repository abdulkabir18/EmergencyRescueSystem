using Application.Common.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Agencies.Queries.GetSupportedIncidents
{
    public class GetAgencySupportedIncidentsQueryHandler : IRequestHandler<GetAgencySupportedIncidentsQuery, Result<List<IncidentType>>>
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetAgencySupportedIncidentsQueryHandler> _logger;

        public GetAgencySupportedIncidentsQueryHandler(
            IAgencyRepository agencyRepository,
            ICacheService cacheService,
            ILogger<GetAgencySupportedIncidentsQueryHandler> logger)
        {
            _agencyRepository = agencyRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<List<IncidentType>>> Handle(GetAgencySupportedIncidentsQuery request, CancellationToken cancellationToken)
        {
            if (request.AgencyId == Guid.Empty)
                return Result<List<IncidentType>>.Failure("Invalid agency id.");

            var cacheKey = $"agency:{request.AgencyId}:supported-incidents";

            try
            {
                var cached = await _cacheService.GetAsync<List<IncidentType>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Supported incidents for agency {AgencyId} retrieved from cache.", request.AgencyId);
                    return Result<List<IncidentType>>.Success(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache lookup failed for supported incidents of agency {AgencyId}", request.AgencyId);
            }

            var agency = await _agencyRepository.GetAsync(request.AgencyId);
            if (agency == null)
            {
                _logger.LogWarning("Agency {AgencyId} not found.", request.AgencyId);
                return Result<List<IncidentType>>.Success([],$"Agency with ID {request.AgencyId} not found.");
            }

            var supported = agency.SupportedIncidents?.ToList() ?? new List<IncidentType>();

            try
            {
                await _cacheService.SetAsync(cacheKey, supported, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache supported incidents for agency {AgencyId}", request.AgencyId);
            }

            return Result<List<IncidentType>>.Success(supported);
        }
    }
}