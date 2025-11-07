using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Incidents.Commands.CreateIncident
{
    public class CreateIncidentCommandHandler : IRequestHandler<CreateIncidentCommand, Result<Guid>>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStorageManager _storageManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateIncidentCommandHandler> _logger;

        public CreateIncidentCommandHandler(IIncidentRepository incidentRepository, ICurrentUserService currentUserService, IStorageManager storageManager, IUnitOfWork unitOfWork, ILogger<CreateIncidentCommandHandler> logger)
        {
            _incidentRepository = incidentRepository;
            _currentUserService = currentUserService;
            _storageManager = storageManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public Task<Result<Guid>> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
