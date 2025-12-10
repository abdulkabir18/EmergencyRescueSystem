using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Users.Queries.GetProfile
{
    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<UserProfileDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;

        public GetProfileQueryHandler(ICacheService cacheService, IUserRepository userRepository, ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _cacheService = cacheService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<UserProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            Guid currentUserId = _currentUserService.UserId;
            if (currentUserId == Guid.Empty)
                return Result<UserProfileDto>.Failure("Unauthorize user");

            string cacheKey = $"GetUserById_{currentUserId}";
            var cached = await _cacheService.GetAsync<Result<UserProfileDto>>(cacheKey);
            if(cached != null) 
                return cached;

            var user = await _userRepository.GetAsync(currentUserId);
            if (user == null)
                return Result<UserProfileDto>.Failure("User not found");

            var dto = new UserProfileDto
            {
                Address = user.Address != null ? new AddressDto 
                { 
                    City = user.Address.City,
                    Country = user.Address.Country,
                    LGA =  user.Address.LGA,
                    PostalCode = user.Address.PostalCode,
                    State = user.Address.State,
                    Street = user.Address.Street
                } : null,
                Email = user.Email.Value,
                FullName = user.FullName,
                Gender = user.Gender.ToString(),
                Id = user.Id,
                AgencyId = user.AgencyId,
                ResponderId = user.ResponderId,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = user.Role.ToString()
            };

            var result = Result<UserProfileDto>.Success(dto);
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
            return result;
        }
    }
}