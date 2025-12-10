using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Commands.ReactivateOrDeactivate
{
    public class ReactivateOrDeactivateCommandHandler : IRequestHandler<ReactivateOrDeactivateCommand, Result<Unit>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReactivateOrDeactivateCommandHandler> _logger;

        public ReactivateOrDeactivateCommandHandler(ICurrentUserService currentUserService, ICacheService cacheService, IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<ReactivateOrDeactivateCommandHandler> logger)
        {
            _currentUserService = currentUserService;
            _cacheService = cacheService;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(ReactivateOrDeactivateCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;
            if (currentUserId == Guid.Empty)
                return Result<Unit>.Failure("Unauthorized user.");

            if (_currentUserService.Role != UserRole.SuperAdmin)
                return Result<Unit>.Failure("Access denied. Only SuperAdmins can perform this action.");

            var targetUser = await _userRepository.GetAsync(request.Model.UserId);
            if (targetUser is null)
                return Result<Unit>.Failure("User not found.");

            if (targetUser.IsActive)
            {
                targetUser.Deactivate();
                _logger.LogInformation("User with ID {TargetUserId} was deactivated by SuperAdmin {AdminId}",targetUser.Id, currentUserId);
            }
            else
            {
                targetUser.Reactivate();
                _logger.LogInformation("User with ID {TargetUserId} was reactivated by SuperAdmin {AdminId}",targetUser.Id, currentUserId);
            }

            await _userRepository.UpdateAsync(targetUser);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveAsync($"GetUserById_{request.Model.UserId}");
            await _cacheService.RemoveByPrefixAsync("GetAllUser");
            await _cacheService.RemoveAsync($"GetUserByEmail_{targetUser.Email.Value}");

            return Result<Unit>.Success(Unit.Value,targetUser.IsActive ? "User reactivated successfully." : "User deactivated successfully.");
        }
    }
}
