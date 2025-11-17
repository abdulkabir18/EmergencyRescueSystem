using Application.Common.Dtos;
using Application.Features.Responders.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Responders.Queries.GetCurrentUserResponder
{
    public class GetCurrentUserResponderQueryHandler : IRequestHandler<GetCurrentUserResponderQuery, Result<ResponderDto>>
    {
        private readonly IResponderRepository _responderRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetCurrentUserResponderQueryHandler> _logger;

        public GetCurrentUserResponderQueryHandler(IResponderRepository responderRepository, ICurrentUserService currentUserService, ICacheService cacheService, ILogger<GetCurrentUserResponderQueryHandler> logger)
        {
            _responderRepository = responderRepository;
            _currentUserService = currentUserService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<ResponderDto>> Handle(GetCurrentUserResponderQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty && _currentUserService.Role != UserRole.Responder)
            {
                _logger.LogWarning("Unauthorized request to get current user's responder profile.");
                return Result<ResponderDto>.Failure("Unauthorized.");
            }

            var cacheKey = $"responder:user:{userId}";

            try
            {
                var cached = await _cacheService.GetAsync<ResponderDto>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Responder profile for user {UserId} returned from cache.", userId);
                    return Result<ResponderDto>.Success(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache lookup failed for responder profile of user {UserId}", userId);
            }

            var responderRef = await _responderRepository.GetAsync(r => r.UserId == userId && !r.IsDeleted);
            if (responderRef == null)
            {
                _logger.LogInformation("Responder profile not found for user {UserId}.", userId);
                return Result<ResponderDto>.Failure("Responder profile not found for current user.");
            }

            var responder = await _responderRepository.GetResponderWithDetailsAsync(responderRef.Id);
            if (responder == null)
            {
                _logger.LogWarning("Responder {ResponderId} not found after lookup.", responderRef.Id);
                return Result<ResponderDto>.Failure("Responder not found.");
            }

            var dto = new ResponderDto
            {
                Id = responder.Id,
                UserId = responder.UserId,
                UserFullName = responder.User?.FullName ?? string.Empty,
                Email = responder.User?.Email.Value ?? string.Empty,
                ProfilePictureUrl = responder.User?.ProfilePictureUrl,
                AgencyId = responder.AgencyId,
                AgencyName = responder.Agency?.Name,
                Status = responder.Status.ToString(),
                Coordinates = responder.Coordinates != null ? new GeoLocationDto(responder.Coordinates.Latitude, responder.Coordinates.Longitude) : null,
                CreatedAt = responder.CreatedAt
            };

            try
            {
                await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache responder profile for user {UserId}", userId);
            }

            _logger.LogInformation("Responder profile for user {UserId} retrieved (ResponderId: {ResponderId}).", userId, responder.Id);
            return Result<ResponderDto>.Success(dto);
        }
    }
}