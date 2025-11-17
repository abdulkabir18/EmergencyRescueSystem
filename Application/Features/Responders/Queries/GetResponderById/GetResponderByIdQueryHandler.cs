using Application.Common.Dtos;
using Application.Features.Responders.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Responders.Queries.GetResponderById
{
    public class GetResponderByIdQueryHandler : IRequestHandler<GetResponderByIdQuery, Result<ResponderDto>>
    {
        private readonly IResponderRepository _responderRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetResponderByIdQueryHandler> _logger;

        public GetResponderByIdQueryHandler(IResponderRepository responderRepository, ICacheService cacheService, ILogger<GetResponderByIdQueryHandler> logger)
        {
            _responderRepository = responderRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<ResponderDto>> Handle(GetResponderByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"responder:{request.ResponderId}";

            try
            {
                var cached = await _cacheService.GetAsync<ResponderDto>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Responder {ResponderId} retrieved from cache.", request.ResponderId);
                    return Result<ResponderDto>.Success(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache lookup failed for responder {ResponderId}", request.ResponderId);
            }

            var responder = await _responderRepository.GetResponderWithDetailsAsync(request.ResponderId);
            if (responder == null)
            {
                _logger.LogWarning("Responder {ResponderId} not found.", request.ResponderId);
                return Result<ResponderDto>.Failure($"Responder with ID {request.ResponderId} not found.");
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
                _logger.LogWarning(ex, "Failed to cache responder {ResponderId}", request.ResponderId);
            }

            _logger.LogInformation("Responder {ResponderId} retrieved.", responder.Id);
            return Result<ResponderDto>.Success(dto);
        }
    }
}