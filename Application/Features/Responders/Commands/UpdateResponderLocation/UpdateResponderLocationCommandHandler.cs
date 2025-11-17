using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Enums;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Responders.Commands.UpdateResponderLocation
{
    public class UpdateResponderLocationCommandHandler : IRequestHandler<UpdateResponderLocationCommand, Result<Unit>>
    {
        private readonly IResponderRepository _responderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UpdateResponderLocationCommandHandler> _logger;

        public UpdateResponderLocationCommandHandler(
            IResponderRepository responderRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICacheService cacheService,
            ILogger<UpdateResponderLocationCommandHandler> logger)
        {
            _responderRepository = responderRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(UpdateResponderLocationCommand request, CancellationToken cancellationToken)
        {
            if (request.Model == null)
                return Result<Unit>.Failure("Invalid request.");

            try
            {
                var responder = await _responderRepository.GetForUpdateAsync(request.ResponderId);
                if (responder == null)
                {
                    _logger.LogWarning("Responder {ResponderId} not found.", request.ResponderId);
                    return Result<Unit>.Failure("Responder not found.");
                }

                var currentUserId = _currentUserService.UserId;
                var currentRole = _currentUserService.Role;
                
                if (currentUserId != responder.UserId &&
                    currentRole != UserRole.SuperAdmin &&
                    currentRole != UserRole.AgencyAdmin)
                {
                    _logger.LogWarning("User {UserId} not authorized to update location for responder {ResponderId}.", currentUserId, request.ResponderId);
                    return Result<Unit>.Failure("Unauthorized.");
                }

                var location = new GeoLocation(request.Model.Latitude, request.Model.Longitude);
                responder.AssignLocation(location);

                await _responderRepository.UpdateAsync(responder);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                try
                {
                    await _cacheService.RemoveAsync($"responder:{responder.Id}");
                    await _cacheService.RemoveAsync($"responder:user:{responder.UserId}");
                    await _cacheService.RemoveByPrefixAsync("responders:");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to invalidate cache after updating responder location for {ResponderId}", responder.Id);
                }

                _logger.LogInformation("Updated location for responder {ResponderId}.", responder.Id);
                return Result<Unit>.Success(Unit.Value, "Location updated.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location for responder {ResponderId}", request.ResponderId);
                return Result<Unit>.Failure("An error occurred while updating location.");
            }
        }
    }
}