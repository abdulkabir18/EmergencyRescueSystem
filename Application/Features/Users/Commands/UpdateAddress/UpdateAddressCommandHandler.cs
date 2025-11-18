using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Commands.UpdateAddress
{
    public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, Result<Unit>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateAddressCommandHandler> _logger;
        private readonly ICacheService _cacheService;

        public UpdateAddressCommandHandler(IUserRepository userRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, ILogger<UpdateAddressCommandHandler> logger, ICacheService cacheService)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<Result<Unit>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Guid currentUserId = _currentUserService.UserId;
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempted to update address.");
                    return Result<Unit>.Failure("User is not authenticated.");
                }

                string cacheKey = $"GetUserById_{currentUserId}";

                var user = await _userRepository.GetAsync(user => user.IsActive && user.Id == currentUserId && !user.IsDeleted);
                if (user == null)
                {
                    _logger.LogWarning("User not found.");
                    return Result<Unit>.Failure("User not found.");
                }

                user.SetAddress(new Address(request.Address.Street!, request.Address.City!, request.Address.State!, request.Address.LGA!, request.Address.Country!, request.Address.PostalCode!));

                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _cacheService.RemoveAsync(cacheKey);
                await _cacheService.RemoveByPrefixAsync("GetAllUser");
                await _cacheService.RemoveAsync($"GetUserByEmail_{user.Email.Value}");

                _logger.LogInformation("Address updated successfully for user {UserId}.", _currentUserService.UserId);
                return Result<Unit>.Success(Unit.Value, "Address updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating address for user {UserId}.", _currentUserService.UserId);
                return Result<Unit>.Failure("An error occurred while updating the address. Please try again later.");
            }
        }
    }
}