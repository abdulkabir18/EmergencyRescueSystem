using System.Threading.Tasks;

namespace Application.Interfaces.External
{
    public interface IGeocodingService
    {
        Task<ReverseGeocodeResult?> GetAddressFromCoordinatesAsync(double latitude, double longitude);
    }

    public record ReverseGeocodeResult(
        string? Street = null,
        string? City = null,
        string? State = null,
        string? LGA = null,
        string? Country = null,
        string? PostalCode = null
    );
}