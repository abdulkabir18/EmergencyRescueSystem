using Application.Common.Dtos;
using Application.Common.Helpers;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Incidents.Commands.CreateIncident
{
    public class CreateIncidentCommandHandler : IRequestHandler<CreateIncidentCommand, Result<Guid>>
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStorageManager _storageManager;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateIncidentCommandHandler> _logger;

        public CreateIncidentCommandHandler(IIncidentRepository incidentRepository, ICurrentUserService currentUserService, IStorageManager storageManager, ICacheService cacheService, IUnitOfWork unitOfWork, ILogger<CreateIncidentCommandHandler> logger)
        {
            _incidentRepository = incidentRepository;
            _currentUserService = currentUserService;
            _storageManager = storageManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cacheService = cacheService;
        }
        public async Task<Result<Guid>> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Guid currentUserId = _currentUserService.UserId;
                if(currentUserId == Guid.Empty)
                {
                    return Result<Guid>.Failure("Unauthorized user.");
                }
                var location = new GeoLocation(request.Model.Coordinate.Latitude, request.Model.Coordinate.Longitude);

                var incident = new Incident(location, request.Model.OccurredAt, currentUserId);

                MediaType mediaType = MediaTypeMapper.MapContentType(request.Model.Prove.ContentType);
                if (!Enum.IsDefined(typeof(MediaType), mediaType))
                {
                    _logger.LogWarning("Unsupported media type: {ContentType}", request.Model.Prove.ContentType);
                     return Result<Guid>.Failure($"Unsupported file type: {request.Model.Prove.ContentType}");
                }

                try
                {
                    var incidentFileUrl = await _storageManager.UploadMediaAsync(request.Model.Prove.OpenReadStream(), request.Model.Prove.FileName, request.Model.Prove.ContentType);
                    incident.AddMedia(incidentFileUrl, mediaType);
                    _logger.LogInformation("Uploaded media file {FileName} for incident.", request.Model.Prove.FileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload media file {FileName}", request.Model.Prove.FileName);
                    return Result<Guid>.Failure(ex.Message);
                }

                await _incidentRepository.AddAsync(incident);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _cacheService.RemoveByPrefixAsync("incidents:");

                _logger.LogInformation("Incident {IncidentId} created at {Time} by user {UserId}. Location: {Latitude}, {Longitude}", incident.Id, DateTime.UtcNow, currentUserId, location.Latitude, location.Longitude);
                return Result<Guid>.Success(incident.Id, "Incident created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an incident.");
                return Result<Guid>.Failure("An unexpected error occurred while creating the incident.");
            }
        }
    }
}