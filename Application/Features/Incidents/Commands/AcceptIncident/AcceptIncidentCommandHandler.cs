using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Common.Exceptions;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Incidents.Commands.AcceptIncident
{
    public class AcceptIncidentCommandHandler : IRequestHandler<AcceptIncidentCommand, Result<Guid>>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IResponderRepository _responderRepository;
        private readonly IIncidentResponderRepository _incidentResponderRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AcceptIncidentCommandHandler> _logger;

        public AcceptIncidentCommandHandler(IIncidentRepository incidentRepository, ICurrentUserService currentUserService, IIncidentResponderRepository incidentResponderRepository, ICacheService cacheService, IResponderRepository responderRepository, IUnitOfWork unitOfWork, ILogger<AcceptIncidentCommandHandler> logger)
        {
            _incidentRepository = incidentRepository;
            _responderRepository = responderRepository;
            _incidentResponderRepository = incidentResponderRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(AcceptIncidentCommand request, CancellationToken cancellationToken)
        {
            Guid userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized access attempt to accept incident {IncidentId}", request.IncidentId);
                return Result<Guid>.Failure("Unauthorized");
            }
            else if (_currentUserService.Role != UserRole.Responder)
            {
                _logger.LogWarning("User {UserId} with role {UserRole} is not a responder and cannot accept incidents.", userId, _currentUserService.Role);
                return Result<Guid>.Failure("Only responders can accept incidents.");
            }

            var incident = await _incidentRepository.GetByIdWithDetailsAsync(request.IncidentId);
            if (incident == null)
            {
                _logger.LogWarning("Incident {IncidentId} not found.", request.IncidentId);
                return Result<Guid>.Failure("Incident not found.");
            }

            var responder = await _responderRepository.GetAsync(r => r.UserId == userId && !r.IsDeleted);
            if (responder == null)
            {
                _logger.LogWarning("Responder profile not found for the current user {UserId}.", userId);
                return Result<Guid>.Failure("Responder profile not found for the current user.");
            }

            if (responder.Status != ResponderStatus.Available)
            {
                _logger.LogWarning("Responder {ResponderId} is not available. Current status: {ResponderStatus}", responder.Id, responder.Status);
                return Result<Guid>.Failure("Responder is not available.");
            }

            if (incident.AssignedResponders.Any(r => r.ResponderId == responder.Id))
            {
                return Result<Guid>.Failure("You have already accepted this incident.");
            }

            int countOfAssigned = incident.AssignedResponders.Count;
            if (countOfAssigned == 0)
            {
                //incident.AssignResponder(responder.Id, ResponderRole.Primary);
                if (incident.Status is IncidentStatus.Resolved or IncidentStatus.Cancelled)
                    throw new BusinessRuleException("Cannot assign responder to a resolved or cancelled incident.");

                var incidentResponder = new IncidentResponder(incident.Id, responder.Id, ResponderRole.Primary);
                await _incidentResponderRepository.AddAsync(incidentResponder);
                incident.MarkAsReport();
                await _incidentRepository.UpdateAsync(incident);
            }
            else if(countOfAssigned <= 3)
            {
                //incident.AssignResponder(responder.Id, ResponderRole.Support);
                if (incident.Status is IncidentStatus.Resolved or IncidentStatus.Cancelled)
                    throw new BusinessRuleException("Cannot assign responder to a resolved or cancelled incident.");

                var incidentResponder = new IncidentResponder(incident.Id, responder.Id, ResponderRole.Support);
                await _incidentResponderRepository.AddAsync(incidentResponder);
            }
            else
            {
                //incident.AssignResponder(responder.Id, ResponderRole.Backup);
                if (incident.Status is IncidentStatus.Resolved or IncidentStatus.Cancelled)
                    throw new BusinessRuleException("Cannot assign responder to a resolved or cancelled incident.");

                var incidentResponder = new IncidentResponder(incident.Id, responder.Id, ResponderRole.Backup);
                await _incidentResponderRepository.AddAsync(incidentResponder);
            }

            responder.UpdateResponderStatus(ResponderStatus.OnDuty);

            await _responderRepository.UpdateAsync(responder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveAsync($"incident:{request.IncidentId}");
            await _cacheService.RemoveByPrefixAsync("responders:");
            await _cacheService.RemoveByPrefixAsync("incidents:");

            _logger.LogInformation("Responder {ResponderId} accepted incident {IncidentId}", responder.Id, incident.Id);
            return Result<Guid>.Success(incident.Id, "Incident accepted successfully.");
        }
    }
}