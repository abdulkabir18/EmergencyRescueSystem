using Application.Common.Dtos;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IIncidentRepository : IGenericRepository<Incident>
    {
        Task<Incident?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Incident>> GetNearbyIncidentsAsync(double latitude, double longitude, double radiusKm);
        Task<bool> ExistsAsync(Guid id);
        Task<PaginatedResult<Incident>> GetAllIncidentsAsync(int pageNumber, int pageSize);
        Task<PaginatedResult<Incident>> GetIncidentsByUserAsync(Guid userId, int pageNumber, int pageSize);
        Task<ICollection<Incident>> GetIncidentsByAgencyIdAsync(Guid agencyId);
        Task<ICollection<Incident>> GetResponderIncidentsAsync(Guid responderId);
    }
}