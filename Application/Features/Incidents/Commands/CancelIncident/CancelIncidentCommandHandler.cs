using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using MediatR;

namespace Application.Features.Incidents.Commands.CancelIncident
{
    public class CancelIncidentCommandHandler : IRequestHandler<CancelIncidentCommand, Result<Guid>>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IResponderRepository _responderRepository;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;

        public CancelIncidentCommandHandler(IIncidentRepository incidentRepository, ICurrentUserService currentUserService, IResponderRepository responderRepository, ICacheService cacheService, IUnitOfWork unitOfWork)
        {
            _incidentRepository = incidentRepository;
            _currentUserService = currentUserService;
            _responderRepository = responderRepository;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CancelIncidentCommand request, CancellationToken cancellationToken)
        {
            if (request.IncidentId == Guid.Empty)
            {
                return Result<Guid>.Failure("Incident id is required.");
            }

            Guid currentUserId = _currentUserService.UserId;
            if (currentUserId == Guid.Empty)
            {
                return Result<Guid>.Failure("Unauthorized user.");
            }

            var incident = await _incidentRepository.GetAsync(request.IncidentId);

            if (incident == null)
                return Result<Guid>.Failure("Incident not found.");

            if (incident.UserId != currentUserId)
            {
                return Result<Guid>.Failure("Only the reporter can cancel the incident.");
            }

            try
            {
                incident.Cancel();

                await _incidentRepository.UpdateAsync(incident);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _cacheService.RemoveAsync($"incident:{request.IncidentId}");
                await _cacheService.RemoveByPrefixAsync("incidents:");

                return Result<Guid>.Success(incident.Id, "Incident cancelled.");
            }
            catch (Exception ex)
            {
                return Result<Guid>.Failure(ex.Message);
            }
        }
    }
}