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

namespace Application.Features.Responders.Queries.GetRespondersByAgency
{
    public class GetRespondersByAgencyQueryHandler : IRequestHandler<GetRespondersByAgencyQuery, Result<PaginatedResult<ResponderDto>>>
    {
        private readonly IResponderRepository _responderRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetRespondersByAgencyQueryHandler> _logger;

        public GetRespondersByAgencyQueryHandler(IResponderRepository responderRepository, ICacheService cacheService, ILogger<GetRespondersByAgencyQueryHandler> logger)
        {
            _responderRepository = responderRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<PaginatedResult<ResponderDto>>> Handle(GetRespondersByAgencyQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"responders:agency:{request.AgencyId}:p{request.PageNumber}:s{request.PageSize}";

            try
            {
                var cached = await _cacheService.GetAsync<PaginatedResult<ResponderDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Responders for agency {AgencyId} page {Page} returned from cache.", request.AgencyId, request.PageNumber);
                    return Result<PaginatedResult<ResponderDto>>.Success(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache lookup failed for agency {AgencyId} responders page {Page}.", request.AgencyId, request.PageNumber);
            }

            var paged = await _responderRepository.GetRespondersByAgencyAsync(request.AgencyId, request.PageNumber, request.PageSize);

            if (paged == null || paged.Data == null || !paged.Data.Any())
                return Result<PaginatedResult<ResponderDto>>.Failure("No responders found for the agency.");

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
                await _cacheService.SetAsync(cacheKey, resultPage, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache agency responders for {AgencyId} page {Page}", request.AgencyId, request.PageNumber);
            }

            return Result<PaginatedResult<ResponderDto>>.Success(resultPage);
        }
    }
}