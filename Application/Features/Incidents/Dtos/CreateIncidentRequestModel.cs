using Application.Common.Dtos;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Incidents.Dtos
{
    public record CreateIncidentRequestModel 
    {
        public GeoLocationDto Coordinate { get; set; } = default!;
        public IFormFile Prove { get; set; } = default!;
        public DateTime OccurredAt { get; set; }
    }
}