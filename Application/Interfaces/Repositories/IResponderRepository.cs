using System.Linq.Expressions;
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
        //Task<IEnumerable<Responder>> GetNearbyRespondersAsync(GeoLocation location, double radiusInKm);
        //Task<IEnumerable<Responder>> GetNearbyRespondersForIncidentAsync(GeoLocation location, IncidentType type, double radiusInKm);
    }
}
