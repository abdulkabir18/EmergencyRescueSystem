using Application.Common.Dtos;
using Application.Features.Incidents.Dtos;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Incidents.Queries.GetResponderIncidents
{
    public class GetResponderIncidentsQueryHandler : IRequestHandler<GetResponderIncidentsQuery, Result<List<IncidentDto>>>
    {
        private readonly IIncidentRepository _incidentRepository;

        public GetResponderIncidentsQueryHandler(IIncidentRepository incidentRepository)
        {
            _incidentRepository = incidentRepository;
        }

        public async Task<Result<List<IncidentDto>>> Handle(GetResponderIncidentsQuery request, CancellationToken cancellationToken)
        {
            if (request.ResponderId == Guid.Empty)
                return Result<List<IncidentDto>>.Failure("Responder ID is required.");

            var incidents = await _incidentRepository.GetResponderIncidentsAsync(request.ResponderId);

            var result = incidents.Select(i => new IncidentDto
            {
                Id = i.Id,
                ReferenceNumber = i.ReferenceCode,
                Title = i.Title,
                Type = i.Type.ToString(),
                Confidence = i.Confidence,
                Status = i.Status.ToString(),
                Coordinates = new GeoLocationDto
                (
                    i.Coordinates.Latitude,
                    i.Coordinates.Longitude
                ),
                Address = i.Address != null ? new AddressDto
                {
                    Street = i.Address.Street,
                    City = i.Address.City,
                    State = i.Address.State,
                    PostalCode = i.Address.PostalCode,
                    Country = i.Address.Country
                } : null,
                OccurredAt = i.OccurredAt,
                UserId = i.UserId,
                UserName = i.User.FullName,
                UserContact = i.User.Email.Value,
                Media = new IncidentMediaInfoDto
                (
                    i.Media.FileUrl,
                    i.Media.MediaType.ToString()
                ),
                AssignedResponders = i.AssignedResponders.Select(ar => new AssignedResponderDto
                {
                    Id = ar.Id,
                    ResponderId = ar.ResponderId,
                    UserId = ar.Responder.UserId,
                    Role = ar.Role.ToString(),
                    ResponderName = ar.Responder.User.FullName
                }).ToList()
            }).ToList();

            return Result<List<IncidentDto>>.Success(result, "Responder incident history retrieved.");
        }
    }
}