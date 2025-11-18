using Application.Common.Dtos;
using Application.Features.Agencies.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Agencies.Queries.GetAgenciesByIncidentType
{
    public class GetAgenciesByIncidentTypeQueryHandler : IRequestHandler<GetAgenciesByIncidentTypeQuery, PaginatedResult<AgencyDto>>
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetAgenciesByIncidentTypeQueryHandler> _logger;

        public GetAgenciesByIncidentTypeQueryHandler(IAgencyRepository agencyRepository, ICacheService cacheService, ILogger<GetAgenciesByIncidentTypeQueryHandler> logger)
        {
            _agencyRepository = agencyRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PaginatedResult<AgencyDto>> Handle(GetAgenciesByIncidentTypeQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
            var cacheKey = $"agencies:incident:{request.Type}:p{pageNumber}:s{pageSize}";

            try
            {
                var cached = await _cacheService.GetAsync<PaginatedResult<AgencyDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Agencies for incident type {Type} returned from cache (page {Page}).", request.Type, pageNumber);
                    return cached;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache lookup failed for agencies by incident type {Type}", request.Type);
            }

            var agenciesRaw = (await _agencyRepository.GetAllAsync(a => !a.IsDeleted && a.SupportedIncidents != null && a.SupportedIncidents.Contains(request.Type))).ToList();

            if (agenciesRaw == null || !agenciesRaw.Any())
            {
                _logger.LogInformation("No agencies found supporting incident type {Type}.", request.Type);
                return PaginatedResult<AgencyDto>.Failure("No agencies found for requested incident type.");
            }

            var totalCount = agenciesRaw.Count;
            var pageItems = agenciesRaw
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = pageItems.Select(a => new AgencyDto(
                a.Id,
                a.AgencyAdminId,
                a.Name,
                a.Email.Value,
                a.PhoneNumber.Value,
                a.LogoUrl,
                a.Address?.ToFullAddress()
            )).ToList();

            var resultPage = PaginatedResult<AgencyDto>.Success(dtos, totalCount, pageNumber, pageSize);

            try
            {
                await _cacheService.SetAsync(cacheKey, resultPage, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache agencies for incident type {Type}", request.Type);
            }

            return resultPage;
        }
    }
}