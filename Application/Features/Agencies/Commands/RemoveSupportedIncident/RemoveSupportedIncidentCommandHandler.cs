using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Agencies.Commands.RemoveSupportedIncident
{
    public class RemoveSupportedIncidentCommandHandler : IRequestHandler<RemoveSupportedIncidentCommand, Result<Guid>>
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<RemoveSupportedIncidentCommandHandler> _logger;

        public RemoveSupportedIncidentCommandHandler(
            IAgencyRepository agencyRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICacheService cacheService,
            ILogger<RemoveSupportedIncidentCommandHandler> logger)
        {
            _agencyRepository = agencyRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(RemoveSupportedIncidentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Guid currentUserId = _currentUserService.UserId;
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempted to remove supported incident.");
                    return Result<Guid>.Failure("User is not authenticated.");
                }

                var agency = await _agencyRepository.GetAsync(a => !a.IsDeleted && a.Id == request.Model.AgencyId);
                if (agency == null)
                {
                    _logger.LogWarning("Agency not found or has been deleted.");
                    return Result<Guid>.Failure("Agency not found or has been deleted.");
                }

                try
                {
                    agency.RemoveSupportedIncident(request.Model.TypeDto.AcceptedIncidentType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while removing supported incident for agency {AgencyId}.", request.Model.AgencyId);
                    return Result<Guid>.Failure(ex.Message);
                }

                await _agencyRepository.UpdateAsync(agency);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                try
                {
                    await _cacheService.RemoveAsync($"agency:{request.Model.AgencyId}:supported-incidents");
                    await _cacheService.RemoveAsync($"agency:{request.Model.AgencyId}");
                    await _cacheService.RemoveByPrefixAsync("agencies:");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to invalidate cache after removing supported incident for agency {AgencyId}", request.Model.AgencyId);
                }

                _logger.LogInformation("Incident type successfully removed for agency {AgencyId}.", request.Model.AgencyId);
                return Result<Guid>.Success(request.Model.AgencyId, "Incident type successfully removed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing supported incident for agency {AgencyId}.", request.Model.AgencyId);
                return Result<Guid>.Failure("An error occurred while removing supported incident. Please try again later.");
            }
        }
    }
}