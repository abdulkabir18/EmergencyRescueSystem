using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Agencies.Commands.AddSupportedIncident
{
    public class AddSupportedIncidentCommandHandler : IRequestHandler<AddSupportedIncidentCommand, Result<Unit>>
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AddSupportedIncidentCommandHandler> _logger;

        public AddSupportedIncidentCommandHandler(IAgencyRepository agencyRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<AddSupportedIncidentCommandHandler> logger)
        {
            _agencyRepository = agencyRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(AddSupportedIncidentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Guid currentUserId = _currentUserService.UserId;
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempted to add supported incident.");
                    return Result<Unit>.Failure("User is not authenticated.");
                }

                var agency = await _agencyRepository.GetAsync(a => !a.IsDeleted && a.Id == request.Model.AgencyId);
                if(agency == null)
                {
                    _logger.LogWarning("Agency not found or has been deleted.");
                    return Result<Unit>.Failure("Agency not found or has been deleted.");
                }

                try
                {
                    agency.AddSupportedIncident(request.Model.TypeDto.AcceptedIncidentType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while adding supported incident for agency {AgencyId}.", request.Model.AgencyId);
                    return Result<Unit>.Failure(ex.Message);
                }

                await _agencyRepository.UpdateAsync(agency);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Incident type successfully added for agency {AgencyId}.", request.Model.AgencyId);
                return Result<Unit>.Success(Unit.Value, "Incident type successfully added.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding supported incident for agency {AgencyId}.", request.Model.AgencyId);
                return Result<Unit>.Failure("An error occurred while adding supported incident. Please try again later.");
            }
        }
    }
}