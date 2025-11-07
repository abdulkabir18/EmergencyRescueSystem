using Application.Common.Interfaces.Repositories;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IIncidentRepository : IGenericRepository<Incident>
    {
        Task<Incident?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Incident>> GetNearbyIncidentsAsync(double latitude, double longitude, double radiusKm);
        Task<bool> ExistsAsync(Guid id);
    }
}
