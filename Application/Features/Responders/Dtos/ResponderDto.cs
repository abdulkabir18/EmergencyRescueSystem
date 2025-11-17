using Application.Common.Dtos;

namespace Application.Features.Responders.Dtos
{
    public record ResponderDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string UserFullName { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string? ProfilePictureUrl { get; init; }
        public Guid AgencyId { get; init; }
        public string? AgencyName { get; init; }
        public string Status { get; init; } = default!;
        public GeoLocationDto? Coordinates { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}