using Application.Common.Dtos;
using Application.Common.Helpers;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Common.Security;
using Domain.Entities;
using Domain.ValueObject;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IStorageService _storageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(IUserRepository userRepository, ICacheService cacheService, IPasswordHasher passwordHasher, IStorageService storageService, IUnitOfWork unitOfWork, ILogger<RegisterUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _cacheService = cacheService;
            _storageService = storageService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting user registration process for email {Email}.", request.Model?.Email);

                if (request.Model == null)
                {
                    _logger.LogWarning("RegisterUserCommand received with a null model.");
                    return Result<Guid>.Failure("Invalid request payload.");
                }

                if (await _userRepository.IsEmailExistAsync(request.Model.Email))
                    return Result<Guid>.Failure($"Email {request.Model.Email} is associated with another account.");

                string fullName = BuildUserFullName.BuildFullName(request.Model.FirstName, request.Model.LastName);
                if (string.IsNullOrEmpty(fullName)) return Result<Guid>.Failure("Name validation failed");

                var user = new User(fullName, new Email(request.Model.Email), request.Model.Gender);

                user.SetPassword($"{request.Model.Password}{user.Id}", _passwordHasher);
                user.SetCreatedBy(user.Id.ToString());

                if (request.Model.ProfilePicture != null)
                {
                    using var stream = request.Model.ProfilePicture.OpenReadStream();
                    string url = await _storageService.UploadAsync(stream, request.Model.ProfilePicture.FileName, request.Model.ProfilePicture.ContentType, "naijarescue/profile-images");

                    user.SetProfilePicture(url);
                }

                await _userRepository.AddAsync(user);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                try
                {
                    await _cacheService.RemoveAsync("User:Count:Total");
                    await _cacheService.RemoveByPrefixAsync("GetAllUser");
                    await _cacheService.RemoveByPrefixAsync("SearchUsers_");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to remove from cache");
                }

                _logger.LogInformation("User {UserId} created successfully.", user.Id);
                return Result<Guid>.Success(user.Id, "User registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during user registration for email {Email}.", request.Model?.Email);
                return Result<Guid>.Failure("An unexpected error occurred during registration.");
            }
        }
    }
}