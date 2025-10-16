using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserByIdQueryHandler> _logger;

        public GetUserByIdQueryHandler(IUserRepository userRepository, ILogger<GetUserByIdQueryHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync(request.UserId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.UserId);
                return Result<UserDto>.Failure($"User with ID {request.UserId} not found");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Address = { City = user.Address.City, Country = user.Address.Country, LGA = user.Address.LGA, PostalCode = user.Address.PostalCode, State = user.Address.State, Street = user.Address.Street },
                Email = user.Email.Value,
                FullName = user.FullName,
                Gender = user.Gender.ToString(),
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = user.Role.ToString()
            };

            return Result<UserDto>.Success(userDto);
        }
    }
}