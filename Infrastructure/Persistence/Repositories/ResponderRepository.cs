using Application.Common.Dtos;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class ResponderRepository : GenericRepository<Responder>, IResponderRepository
    {
        private readonly ProjectDbContext _dbContext;

        public ResponderRepository(ProjectDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        private static double CalculateDistanceKm(GeoLocation loc1, GeoLocation loc2)
        {
            const double R = 6371.0;
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

        public async Task<Responder?> GetResponderWithDetailsAsync(Guid id)
        {
            return await _dbContext.Responders
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Agency)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<PaginatedResult<Responder>> GetAllRespondersAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _dbContext.Responders
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Agency)
                .Where(r => !r.IsDeleted);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<Responder>.Success(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PaginatedResult<Responder>> GetRespondersByAgencyAsync(Guid agencyId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _dbContext.Responders
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Agency)
                .Where(r => !r.IsDeleted && r.AgencyId == agencyId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<Responder>.Success(items, totalCount, pageNumber, pageSize);
        }

        public async Task<IEnumerable<Responder>> GetRespondersByIncidentAsync(Guid incidentId)
        {
            // Assumes IncidentResponder entity exists in the DbContext with Responder navigation
            var responders = await _dbContext.IncidentResponders
                .AsNoTracking()
                .Where(ir => ir.IncidentId == incidentId)
                .Include(ir => ir.Responder)
                    .ThenInclude(r => r.User)
                .Select(ir => ir.Responder!)
                .ToListAsync();

            return responders;
        }

        public async Task<PaginatedResult<Responder>> GetNearbyRespondersAsync(double latitude, double longitude, double radiusKm, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var allWithLocation = await _dbContext.Responders
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Agency)
                .Where(r => !r.IsDeleted && r.Coordinates != null)
                .ToListAsync();

            var origin = new GeoLocation(latitude, longitude);

            var filtered = allWithLocation
                .Select(r => new { Responder = r, DistanceKm = CalculateDistanceKm(r.Coordinates!, origin) })
                .Where(x => x.DistanceKm <= radiusKm)
                .OrderBy(x => x.DistanceKm)
                .ToList();

            var totalCount = filtered.Count;

            var pageItems = filtered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Responder)
                .ToList();

            return PaginatedResult<Responder>.Success(pageItems, totalCount, pageNumber, pageSize);
        }
    }
}