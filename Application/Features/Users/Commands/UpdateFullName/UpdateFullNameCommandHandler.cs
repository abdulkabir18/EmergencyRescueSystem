using Application.Common.Dtos;
using Application.Common.Helpers;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Commands.UpdateFullName
{
    public class UpdateFullNameCommandHandler : IRequestHandler<UpdateFullNameCommand, Result<Unit>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UpdateFullNameCommandHandler> _logger;

        public UpdateFullNameCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentUserService currentUserService, ILogger<UpdateFullNameCommandHandler> logger)
        {
            _userRepository = userRepository;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(UpdateFullNameCommand request, CancellationToken cancellationToken)
        {
            Guid userId = _currentUserService.UserId;
            if(userId == Guid.Empty)
            {
                _logger.LogWarning("Unauthenticated user attempted to update full name.");
                return Result<Unit>.Failure("User is not authenticated.");
            }

            string cacheKey = $"GetUserById_{userId}";

            var user = await _userRepository.GetAsync(u => u.IsActive && !u.IsDeleted && u.Id == userId);
            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return Result<Unit>.Failure("User not found.");
            }

            string fullName = BuildUserFullName.BuildFullName(request.Model.FirstName, request.Model.LastName);

            user.UpdateFullName(fullName);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveAsync(cacheKey);

            _logger.LogInformation("Full name updated successfully for user {UserId}.", userId);
            return Result<Unit>.Success(Unit.Value, "Full name updated successfully.");
        }
    }
}