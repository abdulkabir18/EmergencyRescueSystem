namespace Application.Features.Agencies.Dtos
{
    public record AgencyDto(Guid Id, Guid AgencyAdminId, string Name, string Email, string PhoneNumber, string? LogoUrl, string? Address);
}