using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Dtos;
using Application.Features.Agencies.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Agencies.Queries.SearchAgencies
{
    public class SearchAgenciesQueryHandler : IRequestHandler<SearchAgenciesQuery, PaginatedResult<AgencyDto>>
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<SearchAgenciesQueryHandler> _logger;

        public SearchAgenciesQueryHandler(IAgencyRepository agencyRepository, ICacheService cacheService, ILogger<SearchAgenciesQueryHandler> logger)
        {
            _agencyRepository = agencyRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PaginatedResult<AgencyDto>> Handle(SearchAgenciesQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
            var cacheKey = $"agencies:search:{request.Keyword}:p{pageNumber}:s{pageSize}";

            try
            {
                var cached = await _cacheService.GetAsync<PaginatedResult<AgencyDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Agencies search for '{Keyword}' returned from cache (page {Page}).", request.Keyword, pageNumber);
                    return cached;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache lookup failed for agencies search '{Keyword}'", request.Keyword);
            }

            var agenciesPaged = await _agencyRepository.SearchAgenciesAsync(request.Keyword, pageNumber, pageSize);

            if (agenciesPaged == null || agenciesPaged.Data == null || !agenciesPaged.Data.Any())
            {
                _logger.LogInformation("No agencies matched search '{Keyword}' (page {Page}).", request.Keyword, pageNumber);
                return PaginatedResult<AgencyDto>.Failure("No agencies found for the search criteria.");
            }

            var data = agenciesPaged.Data.Select(a => new AgencyDto(
                a.Id,
                a.AgencyAdminId,
                a.Name,
                a.Email.Value,
                a.PhoneNumber.Value,
                a.LogoUrl,
                a.Address != null ? new AddressDto
                {
                    City = a.Address.City,
                    Country = a.Address.Country,
                    LGA = a.Address.LGA,
                    PostalCode = a.Address.PostalCode,
                    State = a.Address.State,
                    Street = a.Address.Street
                } : null
            )).ToList();

            var resultPage = PaginatedResult<AgencyDto>.Success(data, agenciesPaged.TotalCount, pageNumber, pageSize);

            try
            {
                await _cacheService.SetAsync(cacheKey, resultPage, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache agencies search results for '{Keyword}'", request.Keyword);
            }

            return resultPage;
        }
    }
}