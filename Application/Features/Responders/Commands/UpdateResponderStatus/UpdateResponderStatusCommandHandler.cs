using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Responders.Commands.UpdateResponderStatus
{
    public class UpdateResponderStatusCommandHandler : IRequestHandler<UpdateResponderStatusCommand, Result<Unit>>
    {
        private readonly IResponderRepository _responderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UpdateResponderStatusCommandHandler> _logger;

        public UpdateResponderStatusCommandHandler(
            IResponderRepository responderRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICacheService cacheService,
            ILogger<UpdateResponderStatusCommandHandler> logger)
        {
            _responderRepository = responderRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(UpdateResponderStatusCommand request, CancellationToken cancellationToken)
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
                    _logger.LogWarning("User {UserId} not authorized to update status for responder {ResponderId}.", currentUserId, request.ResponderId);
                    return Result<Unit>.Failure("Unauthorized.");
                }

                responder.UpdateResponderStatus(request.Model.Status);

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
                    _logger.LogWarning(ex, "Failed to invalidate cache after updating responder status for {ResponderId}", responder.Id);
                }

                _logger.LogInformation("Updated status for responder {ResponderId} to {Status}.", responder.Id, request.Model.Status);
                return Result<Unit>.Success(Unit.Value, "Status updated.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for responder {ResponderId}", request.ResponderId);
                return Result<Unit>.Failure("An error occurred while updating status.");
            }
        }
    }
}