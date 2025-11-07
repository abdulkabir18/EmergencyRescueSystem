using Microsoft.AspNetCore.Http;

namespace Application.Features.Incidents.Dtos
{
    public record IncidentMediaDto(ICollection<IFormFile> Files);
}
