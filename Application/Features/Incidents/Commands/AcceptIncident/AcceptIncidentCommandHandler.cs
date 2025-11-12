using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Incidents.Commands.AcceptIncident
{
    public class AcceptIncidentCommandHandler : IRequestHandler<AcceptIncidentCommand, Result<Guid>>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IResponderRepository _responderRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AcceptIncidentCommandHandler> _logger;

        public AcceptIncidentCommandHandler(IIncidentRepository incidentRepository, ICurrentUserService currentUserService, IResponderRepository responderRepository, IUnitOfWork unitOfWork, ILogger<AcceptIncidentCommandHandler> logger)
        {
            _incidentRepository = incidentRepository;
            _responderRepository = responderRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
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

            if (incident.AssignedResponders.Any(r => r.Id == responder.Id))
            {
                return Result<Guid>.Failure("You have already accepted this incident.");
            }
        }
    }
}
