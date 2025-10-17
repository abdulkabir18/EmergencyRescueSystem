using System.Linq.Expressions;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ResponderRepository : GenericRepository<Responder>, IResponderRepository
    {
        private readonly ProjectDbContext _dbContext;

        public ResponderRepository(ProjectDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        private static double CalculateDistance(GeoLocation loc1, GeoLocation loc2)
        {
            const double R = 6371;
            var lat1 = loc1.Latitude * Math.PI / 180.0;
            var lon1 = loc1.Longitude * Math.PI / 180.0;
            var lat2 = loc2.Latitude * Math.PI / 180.0;
            var lon2 = loc2.Longitude * Math.PI / 180.0;

            var dlat = lat2 - lat1;
            var dlon = lon2 - lon1;

            var a = Math.Pow(Math.Sin(dlat / 2), 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2);
            var c = 2 * Math.Asin(Math.Sqrt(a));
            return R * c;
        }

        public async Task<Responder?> GetAsync(Expression<Func<Responder, bool>> expression)
        {
            return await _dbContext.Responders.AsNoTracking().FirstOrDefaultAsync(expression);
        }

        //public async Task<IEnumerable<Responder>> GetNearbyRespondersAsync(GeoLocation location, double radiusInKm)
        //{
        //    if (location == null)
        //        throw new ArgumentNullException(nameof(location), "Incident location is required.");

        //    var responders = await _dbContext.Responders
        //        .AsNoTracking()
        //        .Include(r => r.Agency)
        //        .Include(r => r.User)
        //        .Where(r =>
        //            r.AssignedLocation != null &&
        //            r.Status == ResponderStatus.Available &&
        //            r.IsVerified &&
        //            r.Agency != null &&
        //            r.Agency.IsActive &&
        //            !r.IsDeleted &&
        //            !r.Agency.IsDeleted &&
        //            r.User.IsEmailVerified &&
        //            r.User.IsActive &&
        //            !r.User.IsDeleted)
        //        .ToListAsync();

        //    return responders
        //        .Where(r => CalculateDistance(r.AssignedLocation!, location) <= radiusInKm)
        //        .ToList();
        //}

        //public async Task<IEnumerable<Responder>> GetNearbyRespondersForIncidentAsync(GeoLocation location, IncidentType type, double radiusInKm)
        //{
        //    if (location == null)
        //        throw new ArgumentNullException(nameof(location), "Incident location is required.");

        //    var responders = await _dbContext.Responders
        //        .AsNoTracking()
        //        .AsSplitQuery()
        //        .Include(r => r.User)
        //        .Include(r => r.Agency)
        //            .ThenInclude(a => a.SupportedIncidents)
        //        .Include(r => r.Specialties)
        //        .Include(r => r.Capabilities)
        //        .Where(r =>
        //            r.AssignedLocation != null &&
        //            r.Status == ResponderStatus.Available &&
        //            r.IsVerified &&
        //            r.Agency != null &&
        //            r.Agency.IsActive &&
        //            !r.IsDeleted &&
        //            !r.Agency.IsDeleted &&
        //            r.User.IsEmailVerified &&
        //            r.User.IsActive &&
        //            !r.User.IsDeleted &&
        //            (
        //                r.Agency.SupportedIncidents.Any(si => si.Type == type)
        //                ||
        //                r.Specialties.Any(s => s.Type == type)
        //            ))
        //        .ToListAsync();

        //    return responders
        //        .Where(r => CalculateDistance(r.AssignedLocation!, location) <= radiusInKm)
        //        .ToList();
        //}

        public async Task<Responder?> GetResponderWithDetailsAsync(Guid id)
        {
            return await _dbContext.Responders
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Agency)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

    }
}