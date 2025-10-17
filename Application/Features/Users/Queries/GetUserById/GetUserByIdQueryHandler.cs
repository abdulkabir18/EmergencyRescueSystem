using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserByIdQueryHandler> _logger;
        private readonly ICacheService _cacheService;

        public GetUserByIdQueryHandler(IUserRepository userRepository, ILogger<GetUserByIdQueryHandler> logger, ICacheService cacheService)
        {
            _userRepository = userRepository;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"GetUserById_{request.UserId}";
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("User with ID {UserId} retrieved from cache.", request.UserId);
                return Result<UserDto>.Success(cachedUser);
            }

            var user = await _userRepository.GetAsync(request.UserId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.UserId);
                return Result<UserDto>.Failure($"User with ID {request.UserId} not found");
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

            _logger.LogInformation("User with ID {UserId} retrieved successfully.", request.UserId);

            await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(10));
            return Result<UserDto>.Success(userDto);
        }
    }
}