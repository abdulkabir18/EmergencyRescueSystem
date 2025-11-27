using Application.Common.Dtos;
using Application.Features.Agencies.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Agencies.Queries.GetAgencyById
{
    public class GetAgencyByIdQueryHandler : IRequestHandler<GetAgencyByIdQuery, Result<AgencyDto>>
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetAgencyByIdQueryHandler> _logger;

        public GetAgencyByIdQueryHandler(IAgencyRepository agencyRepository, ICacheService cacheService, ILogger<GetAgencyByIdQueryHandler> logger)
        {
            _agencyRepository = agencyRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<AgencyDto>> Handle(GetAgencyByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"agency:{request.AgencyId}";

            var cached = await _cacheService.GetAsync<AgencyDto>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Agency {AgencyId} retrieved from cache.", request.AgencyId);
                return Result<AgencyDto>.Success(cached);
            }

            var agency = await _agencyRepository.GetAsync(request.AgencyId);
            if (agency == null)
            {
                _logger.LogWarning("Agency {AgencyId} not found.", request.AgencyId);
                return Result<AgencyDto>.Failure($"Agency with ID {request.AgencyId} not found.");
            }

            var dto = new AgencyDto(
                agency.Id,
                agency.AgencyAdminId,
                agency.Name,
                agency.Email.Value,
                agency.PhoneNumber.Value,
                agency.LogoUrl,
                agency.Address != null ? new AddressDto
                {
                    Street = agency.Address.Street,
                    City = agency.Address.City,
                    State = agency.Address.State,
                    PostalCode = agency.Address.PostalCode,
                    Country = agency.Address.Country,
                    LGA = agency.Address.LGA
                } : null
            );

            try
            {
                await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache agency {AgencyId}", request.AgencyId);
            }

            return Result<AgencyDto>.Success(dto);
        }
    }
}