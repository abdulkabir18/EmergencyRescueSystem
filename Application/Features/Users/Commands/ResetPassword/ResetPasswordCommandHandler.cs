using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Common.Security;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<ResetPasswordCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger<ResetPasswordCommandHandler> logger, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Guid userId = _currentUserService.UserId;
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthorized password reset attempt.");
                    return Result<bool>.Failure("Unauthorized. Please log in and try again.");
                }

                var user = await _userRepository.GetAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId}", userId);
                    return Result<bool>.Failure("User not found.");
                }

                if(user.IsPasswordSet())
                {
                    if (!user.VerifyPassword(request.Model.CurrentPassword, _passwordHasher))
                        return Result<bool>.Failure("Current password is incorrect.");
                }

                if (request.Model.NewPassword != request.Model.ConfirmPassword)
                    return Result<bool>.Failure("New password and confirmation do not match.");

                user.ChangePassword(request.Model.NewPassword, _passwordHasher);

                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Password changed successfully for user ID: {UserId}", userId);
                return Result<bool>.Success(true, "Password changed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password for user {UserId}", _currentUserService.UserId);
                return Result<bool>.Failure("An error occurred while resetting the password. Please try again later.");
            }
        }
    }
}