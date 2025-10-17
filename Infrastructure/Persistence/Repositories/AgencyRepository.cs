using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
            return _dbContext.Agencies.AsNoTracking().AnyAsync(x => x.Name == name);
        }

        public Task<bool> IsPhoneNumberExistAsync(string phoneNumber)
        {
            return _dbContext.Agencies.AsNoTracking().AnyAsync(x => x.PhoneNumber == new PhoneNumber(phoneNumber));
        }
    }
}
