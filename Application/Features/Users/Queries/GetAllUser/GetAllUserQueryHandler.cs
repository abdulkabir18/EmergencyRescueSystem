using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Users.Queries.GetAllUser
{
    public class GetAllUserQueryHandler(ICacheService cacheService,
        IUserRepository userRepository) : IRequestHandler<GetAllUserQuery, PaginatedResult<UserDto>>
    {
        public async Task<PaginatedResult<UserDto>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"GetAllUser_{request.PageNumber}_{request.PageSize}";

            var cachedResult = await cacheService.GetAsync<PaginatedResult<UserDto>>(cacheKey);

            if (cachedResult != null)
            {
                return cachedResult;
            }

            var users = await userRepository.GetAllUsersAsync(request.PageNumber, request.PageSize);
            if (users == null || users.Data == null)
            {
                return PaginatedResult<UserDto>.Failure("No users found.");
            }

            var userDtos = users.Data!.Select(user => new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email.Value,
                Role = user.Role.ToString(),
                ProfilePictureUrl = user.ProfilePictureUrl,
                Gender = user.Gender.ToString(),
                IsActive = user.IsActive,
                CreatedAt =user.CreatedAt,
                Address = user.Address != null ? new AddressDto
                {
                    Street = user.Address.Street,
                    City = user.Address.City,
                    State = user.Address.State,
                    PostalCode = user.Address.PostalCode,
                    LGA = user.Address.LGA,
                    Country = user.Address.Country
                } : null,
            }).ToList();
            var result = PaginatedResult<UserDto>.Success(userDtos, users.TotalCount, request.PageNumber, request.PageSize);
            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }
    }
}
