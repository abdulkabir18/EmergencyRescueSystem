using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Commands.SetProfileImage
{
    public class SetProfileImageCommandHandler : IRequestHandler<SetProfileImageCommand, Result<Unit>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStorageService _storageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SetProfileImageCommandHandler> _logger;

        public SetProfileImageCommandHandler(ICacheService cacheService, IUserRepository userRepository, ICurrentUserService currentUserService, IStorageService storageService, IUnitOfWork unitOfWork, ILogger<SetProfileImageCommandHandler> logger)
        {
            _userRepository = userRepository;
            _cacheService = cacheService;
            _currentUserService = currentUserService;
            _storageService = storageService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(SetProfileImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Image == null)
                {
                    _logger.LogWarning("SetProfileImageCommand received with a null model.");
                    return Result<Unit>.Failure("SetProfileImageCommand received with a null model.");
                }
                else if (request.Image.Length >= 5 * 1024 * 1024)
                {
                    _logger.LogWarning("SetProfileImageCommand received with a image bigger than 5 MB");
                    return Result<Unit>.Failure("SetProfileImageCommand received with a image bigger than 5 MB");
                }

                Guid currentUserId = _currentUserService.UserId;
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempted to set profile image.");
                    return Result<Unit>.Failure("User is not authenticated.");
                }

                string cacheKey = $"GetUserById_{currentUserId}";

                var user = await _userRepository.GetAsync(user => user.IsActive && user.Id == currentUserId && !user.IsDeleted);
                if (user == null)
                {
                    _logger.LogWarning("User not found.");
                    return Result<Unit>.Failure("User not found.");
                }

                using var stream = request.Image.OpenReadStream();
                string url = await _storageService.UploadAsync(stream, request.Image.FileName, request.Image.ContentType, "naijarescue/profile-images");

                user.SetProfilePicture(url);

                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _cacheService.RemoveAsync(cacheKey);
                await _cacheService.RemoveByPrefixAsync("GetAllUser");
                await _cacheService.RemoveAsync($"GetUserByEmail_{user.Email.Value}");

                _logger.LogInformation("Profile image set successfully for user {UserId}.", user.Id);

                return Result<Unit>.Success(Unit.Value, "Profile image set successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred when user want to set profile image.");
                return Result<Unit>.Failure("An unexpected error occurred when user want to set profile image.");
            }
        }

    }
}