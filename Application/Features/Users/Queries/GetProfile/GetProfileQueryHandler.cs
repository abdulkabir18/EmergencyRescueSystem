using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Users.Queries.GetProfile
{
    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<UserProfileDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetProfileQueryHandler(IUserRepository userRepository, ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public async Task<Result<UserProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            Guid currentUserId = _currentUserService.UserId;
            if (currentUserId == Guid.Empty)
                return Result<UserProfileDto>.Failure("Unauthorize user");

            var user = await _userRepository.GetAsync(currentUserId);
            if (user == null)
                return Result<UserProfileDto>.Failure("User not found");

            var dto = new UserProfileDto
            {
                Address = user.Address != null ? user.Address.ToFullAddress() : null,
                Email = user.Email.Value,
                FullName = user.FullName,
                Gender = user.Gender.ToString(),
                Id = user.Id,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = user.Role.ToString()
            };
            return Result<UserProfileDto>.Success(dto);
        }
    }
}