using Application.Common.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.ResendVerificationCode
{
    public class ResendVerificationCodeCommandHandler : IRequestHandler<ResendVerificationCodeCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IVerificationService _verificationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ResendVerificationCodeCommandHandler> _logger;

        public ResendVerificationCodeCommandHandler(IUserRepository userRepository, IVerificationService verificationService, IEmailService emailService, ILogger<ResendVerificationCodeCommandHandler> logger)
        {
            _userRepository = userRepository;
            _verificationService = verificationService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(ResendVerificationCodeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetAsync(u => u.Email == new Email(request.Email));
                if (user == null)
                {
                    _logger.LogInformation("ResendVerification requested for non-existent email: {Email}", request.Email);
                    return Result<bool>.Failure("Invalid email.");
                }
                else if (user.IsDeleted || !user.IsActive)
                {
                    _logger.LogInformation("ResendVerification requested for inactive or deleted user: {Email}", request.Email);
                    return Result<bool>.Failure("User account is inactive or deleted.");
                }

                bool canRequestNewCode = await _verificationService.CanRequestNewCodeAsync(user.Id);
                if (!canRequestNewCode)
                    return Result<bool>.Failure("Please wait for some minute before requesting another code.");

                var code = await _verificationService.GenerateCodeAsync(user.Id, 10);

                await _emailService.SendEmailAsync(user.Email.Value, "Your Verification Code", $"Hello {user.FullName},\n\nYour new verification code is: {code}\n\nThis code will expire in 10 minutes.");

                _logger.LogInformation("Verification code resent successfully to {Email}", request.Email);

                return Result<bool>.Success(true, "Verification code has been resent successfully.");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error handling ResendVerificationCodeCommand for email {Email}", request.Email);
                return Result<bool>.Failure("An error occurred while processing your request.");
            }
        }
    }
}
