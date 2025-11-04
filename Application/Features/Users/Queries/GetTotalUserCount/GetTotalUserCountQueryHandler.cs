using Application.Common.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Users.Queries.GetTotalUserCount
{
    public class GetTotalUserCountQueryHandler : IRequestHandler<GetTotalUserCountQuery, Result<int>>
    {
        private readonly ICacheService _cacheService;
        private readonly IUserRepository _userRepository;

        public GetTotalUserCountQueryHandler(ICacheService cacheService, IUserRepository userRepository)
        {
            _cacheService = cacheService;
            _userRepository = userRepository;
        }
        public async Task<Result<int>> Handle(GetTotalUserCountQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "User:Count:Total";
            var cachedResult = await _cacheService.GetAsync<Result<int>>(cacheKey);

            if (cachedResult != null)
            {
                return cachedResult;
            }

            var totalCount = await _userRepository.GetTotalUsersCountAsync();
            var result = Result<int>.Success(totalCount, "Total user count retrieved successfully.");
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }
    }
}