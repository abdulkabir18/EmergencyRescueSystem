using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Queries.GetUserByEmail
{
    public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserByEmailQueryHandler> _logger;
        private readonly ICacheService _cacheService;

        public GetUserByEmailQueryHandler(ICacheService cacheService,IUserRepository userRepository, ILogger<GetUserByEmailQueryHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
            _cacheService = cacheService;
        }
        public async Task<Result<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"GetUserByEmail_{request.Email}";
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("User with email {Email} retrieved from cache.", request.Email);
                return Result<UserDto>.Success(cachedUser);
            }

            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if(user == null || user.IsDeleted)
            {
                _logger.LogWarning("User with email {Email} not found.", request.Email);
                return Result<UserDto>.Failure("User not found.");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email.Value,
                Role = user.Role.ToString(),
                ProfilePictureUrl = user.ProfilePictureUrl,
                Address = user.Address != null ? new AddressDto
                {
                    Street = user.Address.Street,
                    City = user.Address.City,
                    State = user.Address.State,
                    PostalCode = user.Address.PostalCode,
                    LGA = user.Address.LGA,
                    Country = user.Address.Country
                } : null,
                Gender = user.Gender.ToString(),
                IsActive = user.IsActive
            };

            _logger.LogInformation("User with email {Email} retrieved successfully.", request.Email);

            await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(10));
            return Result<UserDto>.Success(userDto);
        }
    }
}