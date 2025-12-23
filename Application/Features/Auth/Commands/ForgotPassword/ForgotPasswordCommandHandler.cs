using Application.Common.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Domain.ValueObject;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IVerificationService _verificationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;

        public ForgotPasswordCommandHandler(IUserRepository userRepository, IVerificationService verificationService, IEmailService emailService, ILogger<ForgotPasswordCommandHandler> logger)
        {
            _userRepository = userRepository;
            _verificationService = verificationService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetAsync(u => u.Email == new Email(request.Email));
                if (user == null)
                {
                    _logger.LogInformation("ForgotPassword requested for non-existent email: {Email}", request.Email);
                    return Result<bool>.Failure("Invalid email.");
                }
                else if (user.IsDeleted || !user.IsActive)
                {
                    _logger.LogInformation("ForgotPassword requested for inactive or deleted user: {Email}", request.Email);
                    return Result<bool>.Failure("User account is inactive or deleted.");
                }

                string verificationCode = await _verificationService.GenerateCodeAsync(user.Id, 10);

                string subject = "EmergencyRescue Password Reset";
                string body = $@"
                <h3>EmergencyRescue Password Reset</h3>
                <p>Hello {user.FullName ?? "User"},</p>
                <p>We received a request to reset your EmergencyRescue account password. 
                Use the verification code below to continue:</p>

                <h2 style='color:#e63946; letter-spacing:2px;'>{verificationCode}</h2>

                <p>This code is valid for <b>15 minutes</b>. If you didn’t request this reset, 
                please ignore this email — your account is still secure.</p>

                <p>Stay safe,<br><b>The EmergencyRescue Team 🚨</b></p>";

                await _emailService.SendEmailAsync(user.Email.Value, subject, body);

                _logger.LogInformation("Password reset code sent to email: {Email}", request.Email);
                return Result<bool>.Success(true, $"OTP to reset your password is been sent to this email {user.Email.Value}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ForgotPasswordCommand for email {Email}", request.Email);
                return Result<bool>.Failure("An error occurred while processing your request.");
            }
        }
    }
}
