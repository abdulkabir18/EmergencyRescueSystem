using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.VerifyUserEmail
{
    public class VerifyUserEmailCommandHandler : IRequestHandler<VerifyUserEmailCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IVerificationService _verificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<VerifyUserEmailCommandHandler> _logger;

        public VerifyUserEmailCommandHandler(IUserRepository userRepository, ICacheService cacheService, IVerificationService verificationService, IUnitOfWork unitOfWork, ILogger<VerifyUserEmailCommandHandler> logger)
        {
            _userRepository = userRepository;
            _verificationService = verificationService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<Result<bool>> Handle(VerifyUserEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting email verification process for {Email}", request.Model?.Email);

                if (request.Model == null)
                {
                    _logger.LogWarning("VerifyUserEmailCommand received with a null model.");
                    return Result<bool>.Failure("Invalid request payload.");
                }

                var user = await _userRepository.GetUserByEmailAsync(request.Model.Email);
                if (user == null)
                {
                    _logger.LogWarning("Email verification failed: User with email {Email} not found.", request.Model.Email);
                    return Result<bool>.Failure("User not found.");
                }

                var isCodeValid = await _verificationService.ValidateCodeAsync(user.Id, request.Model.Code);
                if (!isCodeValid)
                {
                    _logger.LogWarning("Invalid verification code provided for user {UserId}.", user.Id);
                    return Result<bool>.Failure("Invalid or expired verification code.");
                }

                user.VerifyEmail();

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _cacheService.RemoveAsync($"GetUserById_{user.Id}");
                await _cacheService.RemoveByPrefixAsync("GetAllUser");
                await _cacheService.RemoveAsync($"GetUserByEmail_{user.Email.Value}");

                _logger.LogInformation("Email successfully verified for user {UserId}.", user.Id);
                return Result<bool>.Success(true, "User email verified successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during email verification for {Email}", request.Model?.Email);
                return Result<bool>.Failure("An unexpected error occurred during email verification.");
            }
        }
    }
}