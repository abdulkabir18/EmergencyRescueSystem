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

namespace Application.Features.Incidents.Commands.AssignResponder
{
    public class AssignResponderCommandHandler : IRequestHandler<AssignResponderCommand, Result<Guid>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IIncidentRepository _incidentRepository;
        private readonly IIncidentResponderRepository _incidentResponderRepository;
        private readonly IResponderRepository _responderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AssignResponderCommandHandler> _logger;

        public AssignResponderCommandHandler(ICurrentUserService currentUserService, IIncidentResponderRepository incidentResponderRepository, IIncidentRepository incidentRepository, IResponderRepository responderRepository, ICacheService cacheService, IUnitOfWork unitOfWork, ILogger<AssignResponderCommandHandler> logger)
        {
            _currentUserService = currentUserService;
            _incidentResponderRepository = incidentResponderRepository;
            _incidentRepository = incidentRepository;
            _cacheService = cacheService;
            _responderRepository = responderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(AssignResponderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Attempting to assign responder for incident {IncidentId}", request.Model.IncidentId);

                if (request.Model == null)
                {
                    _logger.LogWarning("AssignResponderCommand received with null model.");
                    return Result<Guid>.Failure("Invalid assign responder data.");
                }

                Guid currentUserId = _currentUserService.UserId;
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempted to assign a responder.");
                    return Result<Guid>.Failure("User is not authenticated.");
                }
                else if (_currentUserService.Role != UserRole.SuperAdmin && _currentUserService.Role != UserRole.AgencyAdmin)
                {
                    _logger.LogWarning("User {UserId} with role {UserRole} is not authorized to assign responders to incidents.", currentUserId, _currentUserService.Role);
                    return Result<Guid>.Failure("Only agency admin or super admin can assign responders to incidents.");
                }

                var incident = await _incidentRepository.GetForUpdateAsync(request.Model.IncidentId);
                if (incident == null || incident.IsDeleted)
                {
                    _logger.LogWarning("Incident with ID {IncidentId} not found.", request.Model.IncidentId);
                    return Result<Guid>.Failure("Incident not found.");
                }

                var responder = await _responderRepository.GetForUpdateAsync(request.Model.ResponderId);
                if (responder == null || responder.IsDeleted)
                {
                    _logger.LogWarning("Responder with ID {ResponderId} not found.", request.Model.ResponderId);
                    return Result<Guid>.Failure("Responder not found.");
                }

                if (responder.Status != ResponderStatus.Available)
                {
                    _logger.LogWarning("Responder {ResponderId} is not available.", responder.Id);
                    return Result<Guid>.Failure("Responder is not available.");
                }

                if(incident.AssignedResponders.Any(ir => ir.ResponderId == responder.Id && ir.IsActive))
                {
                    _logger.LogWarning("Responder {ResponderId} is already assigned to incident {IncidentId}.", responder.Id, incident.Id);
                    return Result<Guid>.Failure("Responder is already assigned to this incident.");
                }

                int countOfAssigned = incident.AssignedResponders.Count;

                if (countOfAssigned == 0)
                {
                    if (incident.Status is IncidentStatus.Resolved or IncidentStatus.Cancelled)
                        throw new BusinessRuleException("Cannot assign responder to a resolved or cancelled incident.");

                    var incidentResponder = new IncidentResponder(incident.Id, responder.Id, ResponderRole.Primary);
                    await _incidentResponderRepository.AddAsync(incidentResponder);
                    incident.MarkAsReport();
                }
                else if (countOfAssigned <= 3)
                {
                    if (incident.Status is IncidentStatus.Resolved or IncidentStatus.Cancelled)
                        throw new BusinessRuleException("Cannot assign responder to a resolved or cancelled incident.");

                    var incidentResponder = new IncidentResponder(incident.Id, responder.Id, ResponderRole.Support);
                    await _incidentResponderRepository.AddAsync(incidentResponder);
                }
                else
                {
                    if (incident.Status is IncidentStatus.Resolved or IncidentStatus.Cancelled)
                        throw new BusinessRuleException("Cannot assign responder to a resolved or cancelled incident.");

                    var incidentResponder = new IncidentResponder(incident.Id, responder.Id, ResponderRole.Backup);
                    await _incidentResponderRepository.AddAsync(incidentResponder);
                }

                responder.UpdateResponderStatus(ResponderStatus.OnDuty);
                incident.MarkUpdated();
                responder.MarkUpdated();

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _cacheService.RemoveAsync($"incident:{request.Model.IncidentId}");
                await _cacheService.RemoveByPrefixAsync("responders:");
                await _cacheService.RemoveByPrefixAsync("incidents:");

                _logger.LogInformation("Responder {ResponderId} successfully assigned to incident {IncidentId}.", responder.Id, incident.Id);

                return Result<Guid>.Success(incident.Id, "Responder assigned successfully.");
            }
            catch (BusinessRuleException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while assigning responder to incident {IncidentId}.", request.Model.IncidentId);
                return Result<Guid>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning responder to incident {IncidentId}", request.Model.IncidentId);
                return Result<Guid>.Failure($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}