using Application.Common.Dtos;
using Application.Features.Incidents.Dtos;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Incidents.Queries.GetAgencyIncidents
{
    public class GetAgencyIncidentsQueryHandler : IRequestHandler<GetAgencyIncidentsQuery, Result<List<IncidentDto>>>
    {
        private readonly IIncidentRepository _incidentRepository;

        public GetAgencyIncidentsQueryHandler(IIncidentRepository incidentRepository)
        {
            _incidentRepository = incidentRepository;
        }

        public async Task<Result<List<IncidentDto>>> Handle(GetAgencyIncidentsQuery request, CancellationToken cancellationToken)
        {
            if (request.AgencyId == Guid.Empty)
                return Result<List<IncidentDto>>.Failure("Agency ID is required.");

            var incidents = await _incidentRepository.GetIncidentsByAgencyIdAsync(request.AgencyId);

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
                    ResponderName = ar.Responder.User.FullName,
                    AgencyName = ar.Responder?.Agency.Name
                }).ToList()
            }).ToList();

            return Result<List<IncidentDto>>.Success(result, "Incidents retrieved.");
        }
    }
}