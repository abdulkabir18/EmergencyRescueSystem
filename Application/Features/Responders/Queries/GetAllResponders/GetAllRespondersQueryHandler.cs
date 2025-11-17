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

namespace Application.Features.Responders.Queries.GetAllResponders
{
    public class GetAllRespondersQueryHandler : IRequestHandler<GetAllRespondersQuery, Result<PaginatedResult<ResponderDto>>>
    {
        private readonly IResponderRepository _responderRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetAllRespondersQueryHandler> _logger;

        public GetAllRespondersQueryHandler(IResponderRepository responderRepository, ICacheService cacheService, ILogger<GetAllRespondersQueryHandler> logger)
        {
            _responderRepository = responderRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<PaginatedResult<ResponderDto>>> Handle(GetAllRespondersQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"responders:all:p{request.PageNumber}:s{request.PageSize}";

            try
            {
                var cached = await _cacheService.GetAsync<PaginatedResult<ResponderDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Responders page {Page} retrieved from cache.", request.PageNumber);
                    return Result<PaginatedResult<ResponderDto>>.Success(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache lookup failed for responders page {Page}", request.PageNumber);
            }

            var paged = await _responderRepository.GetAllRespondersAsync(request.PageNumber, request.PageSize);

            if (paged == null || paged.Data == null || !paged.Data.Any())
                return Result<PaginatedResult<ResponderDto>>.Failure("No responders found.");

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
                _logger.LogWarning(ex, "Failed to cache responders page {Page}", request.PageNumber);
            }

            return Result<PaginatedResult<ResponderDto>>.Success(resultPage);
        }
    }
}