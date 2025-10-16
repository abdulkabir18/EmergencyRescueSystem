using Application.Common.Dtos;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Common.Security;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.ConfirmForgotPassword
{
    public class ConfirmForgotPasswordCommandHandler : IRequestHandler<ConfirmForgotPasswordCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IVerificationService _verificationService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ConfirmForgotPasswordCommandHandler> _logger;
        private readonly IEmailService _emailService;

        public ConfirmForgotPasswordCommandHandler(IUserRepository userRepository, IVerificationService verificationService, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork, ILogger<ConfirmForgotPasswordCommandHandler> logger, IEmailService emailService)
        {
            _userRepository = userRepository;
            _verificationService = verificationService;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<Result<bool>> Handle(ConfirmForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetAsync(u => u.Email == new Email(request.Model.Email));
                if (user == null || !user.IsActive || user.IsDeleted)
                {
                    _logger.LogInformation("ResetPassword requested for non-existent email: {Email}", request.Model.Email);
                    return Result<bool>.Failure("Invalid or inactive user.");
                }

                if (request.Model.Code.Length != 6 || !int.TryParse(request.Model.Code, out int r))
                {
                    _logger.LogInformation("ResetPasswordCommand code is not valid");
                    return Result<bool>.Failure("Invalid code");
                }

                bool isValidCode = await _verificationService.ValidateCodeAsync(user.Id, request.Model.Code);
                if (!isValidCode)
                {
                    _logger.LogWarning("Invalid or expired password reset code for user {UserId}", user.Id);
                    return Result<bool>.Failure("Invalid or expired reset code.");
                }

                if (request.Model.NewPassword != request.Model.ConfirmPassword)
                    return Result<bool>.Failure("Password did not match");

                user.ChangePassword(request.Model.NewPassword, _passwordHasher);

                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                string subject = "✅ Password Reset Successful";
                string body = $@"
                    <h3>Password Reset Confirmation</h3>
                    <p>Hello {user.FullName ?? "User"},</p>
                    <p>Your password has been successfully reset. 
                    If you did not initiate this action, please contact support immediately.</p>
                    <p>Stay safe,<br><b>The NaijaRescue Team 🚨</b></p>";

                await _emailService.SendEmailAsync(user.Email.Value, subject, body);

                _logger.LogInformation("Password reset successful for user {Email}", request.Model.Email);

                return Result<bool>.Success(true, "Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ResetPasswordCommand for email {Email}", request.Model.Email);
                return Result<bool>.Failure("An error occurred while resetting the password.");
            }
        }
    }
}