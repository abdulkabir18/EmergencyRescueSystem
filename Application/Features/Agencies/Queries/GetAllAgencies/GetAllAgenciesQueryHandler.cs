using Application.Common.Dtos;
using Application.Features.Agencies.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Agencies.Queries.GetAllAgencies
{
    public class GetAllAgenciesQueryHandler : IRequestHandler<GetAllAgenciesQuery, PaginatedResult<AgencyDto>>
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly ICacheService _cacheService;

        public GetAllAgenciesQueryHandler(IAgencyRepository agencyRepository, ICacheService cacheService)
        {
            _agencyRepository = agencyRepository;
            _cacheService = cacheService;
        }

        public async Task<PaginatedResult<AgencyDto>> Handle(GetAllAgenciesQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"agencies:p {request.Model.PageNumber}:s {request.Model.PageSize}";

            var cached = await _cacheService.GetAsync<PaginatedResult<AgencyDto>>(cacheKey);
            if (cached != null)
                return cached;

            var agencies  = await _agencyRepository.GetAllAgenciesAsync(request.Model.PageNumber, request.Model.PageSize);

            if (agencies == null || !agencies.Data.Any())
                return PaginatedResult<AgencyDto>.Failure("No agencies found.");

            var data = agencies.Data.Select(a => new AgencyDto(
                a.Id,
                a.AgencyAdminId,
                a.Name,
                a.Email.Value,
                a.PhoneNumber.Value,
                a.LogoUrl,
                a.Address?.ToFullAddress()
            )).ToList();

            var result = PaginatedResult<AgencyDto>.Success(data, agencies.TotalCount, request.Model.PageNumber, request.Model.PageSize);

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));

            return result;
        }
    }
}