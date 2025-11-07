using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class IncidentRepository : GenericRepository<Incident>, IIncidentRepository
    {
        private readonly ProjectDbContext _dbContext;
        public IncidentRepository(ProjectDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.Incidents.AnyAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<Incident?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbContext.Incidents
                .Include(i => i.AssignedResponders)
                    .ThenInclude(r => r.Responder)
                    .ThenInclude(res => res.User)
                .Include(i => i.Medias)
                .AsSplitQuery()
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Incident>> GetNearbyIncidentsAsync(double latitude, double longitude, double radiusKm)
        {
            const double EarthRadiusKm = 6371.0;

            return await _dbContext.Incidents
                .Where(i => !i.IsDeleted &&
                            (EarthRadiusKm * Math.Acos(
                                Math.Cos(Math.PI * latitude / 180.0) *
                                Math.Cos(Math.PI * i.Coordinates.Latitude / 180.0) *
                                Math.Cos(Math.PI * i.Coordinates.Longitude / 180.0 - Math.PI * longitude / 180.0) +
                                Math.Sin(Math.PI * latitude / 180.0) *
                                Math.Sin(Math.PI * i.Coordinates.Latitude / 180.0)
                            )) <= radiusKm)
                .ToListAsync();
        }
    }
}