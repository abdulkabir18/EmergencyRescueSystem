using System.Linq.Expressions;
using Application.Common.Dtos;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Interfaces.Repositories
{
    public interface IResponderRepository : IGenericRepository<Responder>
    {
        Task<Responder?> GetAsync(Expression<Func<Responder, bool>> expression);
        Task<Responder?> GetResponderWithDetailsAsync(Guid id);

        // Paging and listing helpers
        Task<PaginatedResult<Responder>> GetAllRespondersAsync(int pageNumber, int pageSize);
        Task<PaginatedResult<Responder>> GetRespondersByAgencyAsync(Guid agencyId, int pageNumber, int pageSize);

        // Assigned to an incident (no paging - usually small)
        Task<IEnumerable<Responder>> GetRespondersByIncidentAsync(Guid incidentId);

        // Nearby responders (paginated)
        Task<PaginatedResult<Responder>> GetNearbyRespondersAsync(double latitude, double longitude, double radiusKm, int pageNumber, int pageSize);
    }
}
