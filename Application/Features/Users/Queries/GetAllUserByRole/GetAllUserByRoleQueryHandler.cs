using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Users.Queries.GetAllUserByRole
{
    public class GetAllUserByRoleQueryHandler(ICacheService cacheService,
        IUserRepository userRepository) : IRequestHandler<GetAllUserByRoleQuery, PaginatedResult<UserDto>>
    {
        public async Task<PaginatedResult<UserDto>> Handle(GetAllUserByRoleQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"GetAllUserByRole_{request.Role}_{request.PageNumber}_{request.PageSize}";

            var cachedResult = await cacheService.GetAsync<PaginatedResult<UserDto>>(cacheKey);

            if (cachedResult != null)
            {
                return cachedResult;
            }
            var users = await userRepository.GetAllUsersByRoleAsync(request.Role, request.PageNumber, request.PageSize);

            var userDtos = users.Data!.Select(user => new UserDto
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
            }).ToList();
            var result = PaginatedResult<UserDto>.Create(userDtos, users.TotalCount, request.PageNumber, request.PageSize);
            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }
    }
}
