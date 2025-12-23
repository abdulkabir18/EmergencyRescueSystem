using Application.Common.Dtos;
using Application.Features.Auth.Dtos;
using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObject;
using Google.Apis.Auth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.ContinueWithGoogle
{

    public class ContinueWithGoogleCommandHandler : IRequestHandler<ContinueWithGoogleCommand, Result<LoginResponseModel>>
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContinueWithGoogleCommandHandler> _logger;

        public ContinueWithGoogleCommandHandler(IAuthService authService,ILogger<ContinueWithGoogleCommandHandler> logger, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _userRepository = userRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<LoginResponseModel>> Handle(ContinueWithGoogleCommand request, CancellationToken cancellationToken)
        {
            GoogleJsonWebSignature.Payload? payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(request.Model.AccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate Google access token.");
                return Result<LoginResponseModel>.Failure("Invalid Google token.");
            }

            if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
            {
                _logger.LogWarning("Google token payload is null or email is empty.");

                return Result<LoginResponseModel>.Failure("Invalid Google token.");
            }

            var user = await _userRepository.GetAsync(x => x.Email == new Email(payload.Email));
            if (user == null)
            {
                user = User.RegisterWithGoogle(payload.Name,new Email(payload.Email),payload.Picture,Gender.Other);
                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (!user.IsEmailVerified)
            {
                user.VerifyEmail();
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            string token = _authService.GenerateToken(user.Id, user.Email.Value, user.Role.ToString());
            return Result<LoginResponseModel>.Success(new LoginResponseModel(token, user.IsEmailVerified), "Login successful.");
        }
    }
}
