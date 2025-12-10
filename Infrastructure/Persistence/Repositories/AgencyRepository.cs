using Application.Common.Dtos;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Persistence.Repositories
{
    public class AgencyRepository : GenericRepository<Agency>, IAgencyRepository
    {
        private readonly ProjectDbContext _dbContext;
        public AgencyRepository(ProjectDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public Task<Agency?> GetAsync(Expression<Func<Agency, bool>> expression)
        {
            return _dbContext.Agencies.AsNoTracking().FirstOrDefaultAsync(expression);
        }

        public Task<bool> IsEmailExistAsync(string email)
        {
            return _dbContext.Agencies.AsNoTracking().AnyAsync(x => x.Email == new Email(email));
        }

        public Task<bool> IsAgencyExist(Guid AgencyId)
        {
            return _dbContext.Agencies.AsNoTracking().AnyAsync(x => x.Id == AgencyId);
        }

        public Task<bool> IsNameExistAsync(string name)
        {
            return _dbContext.Agencies.AsNoTracking().AnyAsync(x => x.Name.ToLower() == name.ToLower());
        }

        public Task<bool> IsPhoneNumberExistAsync(string phoneNumber)
        {
            return _dbContext.Agencies.AsNoTracking().AnyAsync(x => x.PhoneNumber == new PhoneNumber(phoneNumber));
        }

        public async Task<PaginatedResult<Agency>> GetAllAgenciesAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Agencies.AsNoTracking().Where(u => !u.IsDeleted);

            var totalCount = await query.CountAsync();

            var agencies = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<Agency>.Success(agencies, totalCount, pageNumber, pageSize);
        }

        public async Task<PaginatedResult<Agency>> SearchAgenciesAsync(string keyword, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAllAgenciesAsync(pageNumber, pageSize);

            keyword = keyword.Trim().ToLower();

            var query = _dbContext.Agencies
                .AsNoTracking()
                .Where(a => !a.IsDeleted &&
                            (
                                EF.Functions.Like(a.Name.ToLower(), $"%{keyword}%")
                                || (a.Address != null && (
                                    EF.Functions.Like(a.Address.Street.ToLower() ?? string.Empty, $"%{keyword}%")
                                    || EF.Functions.Like(a.Address.City.ToLower() ?? string.Empty, $"%{keyword}%")
                                    || EF.Functions.Like(a.Address.State.ToLower() ?? string.Empty, $"%{keyword}%")
                                    || EF.Functions.Like(a.Address.Country.ToLower() ?? string.Empty, $"%{keyword}%")
                                ))
                            ));

            try
            {
                query = query.Where(a =>
                    EF.Functions.Like(a.Name.ToLower(), $"%{keyword}%")
                    || EF.Functions.Like(a.Email, $"%{keyword}%")
                    || EF.Functions.Like(a.PhoneNumber, $"%{keyword}%")
                    || (a.Address != null && (
                        EF.Functions.Like(a.Address.Street.ToLower() ?? string.Empty, $"%{keyword}%")
                        || EF.Functions.Like(a.Address.City.ToLower() ?? string.Empty, $"%{keyword}%")
                        || EF.Functions.Like(a.Address.State.ToLower() ?? string.Empty, $"%{keyword}%")
                        || EF.Functions.Like(a.Address.Country.ToLower() ?? string.Empty, $"%{keyword}%")
                    )));
            }
            catch
            {

            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<Agency>.Success(items, totalCount, pageNumber, pageSize);
        }

        public async Task<IEnumerable<Agency>> GetAgenciesBySupportedIncidentAsync(IncidentType incidentType)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            var typeJson = JsonSerializer.Serialize(new[] { incidentType }, jsonOptions);

            return await _dbContext.Agencies
                .Where(a => !a.IsDeleted &&
                            EF.Functions.JsonContains(a.SupportedIncidents, typeJson))
                .ToListAsync();
        }
    }
}
