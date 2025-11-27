using Application.Common.Dtos;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                .Include(i => i.User)
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

        public async Task<PaginatedResult<Incident>> GetAllIncidentsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _dbContext.Incidents
                .AsNoTracking()
                .Where(i => !i.IsDeleted)
                .Include(i => i.Medias)
                .Include(i => i.User)
                .Include(i => i.AssignedResponders)
                    .ThenInclude(ar => ar.Responder)
                        .ThenInclude(r => r.User)
                .AsSplitQuery()
                .OrderByDescending(i => i.OccurredAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<Incident>.Success(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PaginatedResult<Incident>> GetIncidentsByUserAsync(Guid userId, int pageNumber, int pageSize)
        {
            if (userId == Guid.Empty)
                return PaginatedResult<Incident>.Success([], 0, pageNumber < 1 ? 1 : pageNumber, pageSize < 1 ? 10 : pageSize);

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _dbContext.Incidents
                .AsNoTracking()
                .Where(i => !i.IsDeleted && i.UserId == userId)
                .Include(i => i.User)
                .Include(i => i.Medias)
                .Include(i => i.AssignedResponders)
                    .ThenInclude(ar => ar.Responder)
                        .ThenInclude(r => r.User)
                .AsSplitQuery()
                .OrderByDescending(i => i.OccurredAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<Incident>.Success(items, totalCount, pageNumber, pageSize);
        }

        public async Task<ICollection<Incident>> GetIncidentsByAgencyIdAsync(Guid agencyId)
        {
            return await _dbContext.Incidents
            .Where(i => i.AssignedResponders
                .Any(ar => ar.Responder.AgencyId == agencyId))
            .Include(i => i.AssignedResponders)
                .ThenInclude(ar => ar.Responder)
            .Include(i => i.Medias)
            .Include(i => i.Address)
            .ToListAsync();
        }
    }
}