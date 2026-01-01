using Application.Common.Dtos;
using Application.Common.Helpers;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Common.Security;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObject;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Responders.Commands.RegisterResponder
{
    public class RegisterResponderCommandHandler : IRequestHandler<RegisterResponderCommand, Result<Guid>>
    {
        private readonly IResponderRepository _responderRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;
        private readonly IAgencyRepository _agencyRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IStorageService _storageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<RegisterResponderCommandHandler> _logger;

        public RegisterResponderCommandHandler(
            IResponderRepository responderRepository,
            ICurrentUserService currentUserService,
            IUserRepository userRepository,
            IAgencyRepository agencyRepository,
            IPasswordHasher passwordHasher,
            IStorageService storageService,
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<RegisterResponderCommandHandler> logger)
        {
            _responderRepository = responderRepository;
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _agencyRepository = agencyRepository;
            _passwordHasher = passwordHasher;
            _storageService = storageService;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
        }
        public async Task<Result<Guid>> Handle(RegisterResponderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting responder registration process.");

                if (request.Model == null)
                {
                    _logger.LogWarning("RegisterResponderCommand received with a null model.");
                    return Result<Guid>.Failure("Invalid request payload.");
                }

                Guid currentUserId = _currentUserService.UserId;
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempted to register a responder.");
                    return Result<Guid>.Failure("User is not authenticated.");
                }

                if (!await _agencyRepository.IsAgencyExist(request.Model.AgencyId))
                    return Result<Guid>.Failure("Agency not found or inactive.");

                if (await _userRepository.IsEmailExistAsync(request.Model.RegisterUserRequest.Email))
                    return Result<Guid>.Failure($"Email {request.Model.RegisterUserRequest.Email} already exists.");

                string fullName = BuildUserFullName.BuildFullName(request.Model.RegisterUserRequest.FirstName, request.Model.RegisterUserRequest.LastName);
                if (string.IsNullOrEmpty(fullName)) return Result<Guid>.Failure("Name validation failed");

                var user = new User(fullName, new Email(request.Model.RegisterUserRequest.Email), request.Model.RegisterUserRequest.Gender, UserRole.Responder);

                user.SetPassword($"{request.Model.RegisterUserRequest.Password}{user.Id}", _passwordHasher);

                if (request.Model.RegisterUserRequest.ProfilePicture != null)
                {
                    using var stream = request.Model.RegisterUserRequest.ProfilePicture.OpenReadStream();
                    string imageUrl = await _storageService.UploadAsync(stream, request.Model.RegisterUserRequest.ProfilePicture.FileName, request.Model.RegisterUserRequest.ProfilePicture.ContentType, "naijarescue/profile-images");

                    user.SetProfilePicture(imageUrl);
                }

                user.SetCreatedBy(currentUserId.ToString());

                var responder = new Responder(user.Id, request.Model.AgencyId);

                if (request.Model.AssignedLocation != null)
                    responder.AssignLocation(new GeoLocation(request.Model.AssignedLocation.Latitude, request.Model.AssignedLocation.Longitude));

                user.AssignAsResponder(responder.Id);
                responder.SetCreatedBy(currentUserId.ToString());

                await _userRepository.AddAsync(user);
                await _responderRepository.AddAsync(responder);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                try
                {
                    await _cacheService.RemoveByPrefixAsync("responders:");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to invalidate responder caches after registration for responder {ResponderId}", responder.Id);
                }

                _logger.LogInformation("Responder {ResponderId} for user {UserId} created successfully by {CurrentUserId}.", responder.Id, user.Id, currentUserId);
                return Result<Guid>.Success(responder.Id, "Responder registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during responder registration.");
                return Result<Guid>.Failure("An unexpected error occurred while registering the responder.");
            }
        }
    }
}