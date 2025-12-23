using Application.Common.Dtos;
using Application.Features.Auth.Dtos;
using Application.Interfaces.Auth;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Domain.Common.Security;
using Domain.Common.Templates;
using Domain.ValueObject;
using MediatR;

namespace Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseModel>>
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly IVerificationService _verificationService;

        public LoginCommandHandler(IAuthService authService, IUserRepository userRepository, IPasswordHasher passwordHasher, IEmailService emailService, IVerificationService verificationService)
        {
            _authService = authService;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _verificationService = verificationService;
        }

        public async Task<Result<LoginResponseModel>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if(request == null || request.Model == null)
                return Result<LoginResponseModel>.Failure("Invalid login request.");

            var user = await _userRepository.GetAsync(x => x.Email == new Email(request.Model.Email));
            if (user == null)
                return Result<LoginResponseModel>.Failure("Invalid email or password.");

            if(!user.VerifyPassword($"{request.Model.Password}{user.Id}", _passwordHasher))
                return Result<LoginResponseModel>.Failure("Invalid email or password.");

            if (!user.IsEmailVerified)
            {
                string code = await _verificationService.GenerateCodeAsync(user.Id);
                var emailResult = await _emailService.SendEmailAsync(user.Email.Value, EmailTemplates.GetVerificationSubject(), EmailTemplates.GetVerificationBody(code));
                
                string message = emailResult.Succeeded
                    ? "A new verification code has been sent to your inbox." 
                    : $"Failed to send verification email. {emailResult.Message}";

                return Result<LoginResponseModel>.Success(new LoginResponseModel(string.Empty, user.IsEmailVerified), $"Email not verified. \n{message}");
            }

            string token = _authService.GenerateToken(user.Id, user.Email.Value, user.Role.ToString());
            return Result<LoginResponseModel>.Success(new LoginResponseModel(token, user.IsEmailVerified), "Login successful.");
        }
    }
}
