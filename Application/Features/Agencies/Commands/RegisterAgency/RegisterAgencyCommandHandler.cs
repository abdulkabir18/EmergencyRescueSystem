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

namespace Application.Features.Agencies.Commands.RegisterAgency
{
    public class RegisterAgencyCommandHandler : IRequestHandler<RegisterAgencyCommand, Result<Guid>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAgencyRepository _agencyRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IStorageService _storageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RegisterAgencyCommandHandler> _logger;

        public RegisterAgencyCommandHandler(IUserRepository userRepository, IAgencyRepository agencyRepository, IPasswordHasher passwordHasher, IStorageService storageService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, ILogger<RegisterAgencyCommandHandler> logger)
        {
            _userRepository = userRepository;
            _agencyRepository = agencyRepository;
            _passwordHasher = passwordHasher;
            _storageService = storageService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result<Guid>> Handle(RegisterAgencyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting agency registration process.");

                if (request.Model == null)
                {
                    _logger.LogWarning("RegisterAgencyCommand received with null model.");
                    return Result<Guid>.Failure("Invalid request payload.");
                }

                Guid currentUserId = _currentUserService.UserId;
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempted to register an agency.");
                    return Result<Guid>.Failure("User is not authenticated.");
                }

                if (await _userRepository.IsEmailExistAsync(request.Model.RegisterUserRequest.Email))
                    return Result<Guid>.Failure($"Email {request.Model.RegisterUserRequest.Email} is associated with another account.");
                if (await _agencyRepository.IsNameExistAsync(request.Model.AgencyName))
                    return Result<Guid>.Failure($"Agency Name {request.Model.AgencyName} is associated with another account.");
                if (await _agencyRepository.IsEmailExistAsync(request.Model.AgencyEmail))
                    return Result<Guid>.Failure($"Agency Email {request.Model.AgencyEmail} is associated with another account.");
                if (await _agencyRepository.IsPhoneNumberExistAsync(request.Model.AgencyPhoneNumber))
                    return Result<Guid>.Failure($"Agency PhoneNumber {request.Model.AgencyPhoneNumber} is associated with another account.");

                string fullName = BuildUserFullName.BuildFullName(request.Model.RegisterUserRequest.FirstName, request.Model.RegisterUserRequest.LastName);
                if (string.IsNullOrEmpty(fullName)) return Result<Guid>.Failure("Name validation failed");

                var user = new User(fullName, new Email(request.Model.RegisterUserRequest.Email), request.Model.RegisterUserRequest.Gender, UserRole.AgencyAdmin);

                user.SetPassword($"{request.Model.RegisterUserRequest.Password}{user.Id}", _passwordHasher);

                if (request.Model.RegisterUserRequest.ProfilePicture != null)
                {
                    using var stream = request.Model.RegisterUserRequest.ProfilePicture.OpenReadStream();
                    string imageUrl = await _storageService.UploadAsync(stream, request.Model.RegisterUserRequest.ProfilePicture.FileName, request.Model.RegisterUserRequest.ProfilePicture.ContentType, "naijarescue/profile-images");

                    user.SetProfilePicture(imageUrl);
                }

                user.SetCreatedBy(currentUserId.ToString());

                var agency = new Agency(user.Id, request.Model.AgencyName, new Email(request.Model.AgencyEmail), new PhoneNumber(request.Model.AgencyPhoneNumber));

                if (request.Model.AgencyAddress != null)
                {
                    agency.SetAddress(new Address(request.Model.AgencyAddress.Street!, request.Model.AgencyAddress.City!, request.Model.AgencyAddress.State!, request.Model.AgencyAddress.LGA!, request.Model.AgencyAddress.Country!, request.Model.AgencyAddress.PostalCode!));
                }

                if (request.Model.AgencyLogo != null)
                {
                    using var stream = request.Model.AgencyLogo.OpenReadStream();
                    string imageUrl = await _storageService.UploadAsync(stream, request.Model.AgencyLogo.FileName, request.Model.AgencyLogo.ContentType, "naijarescue/agency-logos");

                    agency.SetLogo(imageUrl);
                }

                foreach (var incident in request.IncidentTypes.SupportedIncidents)
                {
                    if (!Enum.IsDefined(typeof(IncidentType), incident.AcceptedIncidentType))
                        return Result<Guid>.Failure($"IncidentType '{incident.AcceptedIncidentType}' is not valid.");

                    agency.AddSupportedIncident(incident.AcceptedIncidentType);
                }

                user.AssignToAgency(agency.Id);
                agency.SetCreatedBy(currentUserId.ToString());

                await _userRepository.AddAsync(user);
                await _agencyRepository.AddAsync(agency);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Agency {AgencyId} created successfully by user {UserId}.", agency.Id, currentUserId);
                return Result<Guid>.Success(agency.Id, "Agency registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering agency.");
                return Result<Guid>.Failure("An error occurred while registering the agency.");
            }
        }
    }
}
