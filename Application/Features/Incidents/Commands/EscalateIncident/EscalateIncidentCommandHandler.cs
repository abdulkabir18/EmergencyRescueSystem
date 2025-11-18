using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using MediatR;

namespace Application.Features.Incidents.Commands.EscalateIncident
{
    public class EscalateIncidentCommandHandler : IRequestHandler<EscalateIncidentCommand, Result<Guid>>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IResponderRepository _responderRepository;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;

        public EscalateIncidentCommandHandler(IIncidentRepository incidentRepository, ICurrentUserService currentUserService, ICacheService cacheService, IResponderRepository responderRepository, IUnitOfWork unitOfWork)
        {
            _incidentRepository = incidentRepository;
            _currentUserService = currentUserService;
            _cacheService = cacheService;
            _responderRepository = responderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(EscalateIncidentCommand request, CancellationToken cancellationToken)
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

            var responder = await _responderRepository.GetAsync(r => r.UserId == currentUserId && !r.IsDeleted);
            if (responder == null)
            {
                return Result<Guid>.Failure("Unauthorized user.");
            }

            var incident = await _incidentRepository.GetAsync(request.IncidentId);

            if (incident == null)
                return Result<Guid>.Failure("Incident not found.");

            if (incident.AssignedResponders.Any(ir => ir.ResponderId != responder.Id || !ir.IsActive))
            {
                return Result<Guid>.Failure("Responder not assigned to this incident");
            }

            try
            {
                incident.Escalate();

                await _incidentRepository.UpdateAsync(incident);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _cacheService.RemoveAsync($"incident:{request.IncidentId}");
                await _cacheService.RemoveByPrefixAsync("responders:");
                await _cacheService.RemoveByPrefixAsync("incidents:");

                return Result<Guid>.Success(incident.Id, "Incident escalated.");
            }
            catch (Exception ex)
            {
                return Result<Guid>.Failure(ex.Message);
            }
        }
    }
}
