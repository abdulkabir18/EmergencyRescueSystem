using Application.Common.Dtos;
using System;
using System.Collections.Generic;

namespace Application.Features.Incidents.Dtos
{
    public record AssignedResponderDto
    {
        public Guid Id { get; set; }
        public Guid ResponderId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? ResponderName { get; set; }
        public string? AgencyName { get; set; }
    }

    public record IncidentDto
    {
        public Guid Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string Type { get; set; } = string.Empty;
        public double? Confidence { get; set; }
        public string Status { get; set; } = string.Empty;
        public GeoLocationDto Coordinates { get; set; } = default!;
        public AddressDto? Address { get; set; }
        public DateTime OccurredAt { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserContact { get; set; } = string.Empty;
        public IncidentMediaInfoDto Media { get; set; } = default!;
        public List<AssignedResponderDto> AssignedResponders { get; set; } = new();
    }
}