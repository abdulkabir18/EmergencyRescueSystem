using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ProjectDbContext _dbContext;

        public UserRepository(ProjectDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<User?> GetAsync(Expression<Func<User, bool>> expression)
        {
            return _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(expression);
        }

        //public async Task<IEnumerable<string>> GetEmergencyContactEmailsAsync(Guid userId)
        //{
        //    var user = await _dbContext.Users
        //        .AsNoTracking()
        //        .Include(u => u.EmergencyContacts)
        //        .FirstOrDefaultAsync(u => u.Id == userId);

        //    if (user == null || user.EmergencyContacts == null)
        //        return [];

        //    return user.EmergencyContacts
        //        .Where(c => c.Email != null)
        //        .Select(c => c.Email.Value)
        //        .Distinct()
        //        .ToList();
        //}

        public Task<User?> GetUserByEmailAsync(string email)
        {
            return _dbContext.Users.FirstOrDefaultAsync(x => x.Email == new Email(email));
        }

        public Task<bool> IsEmailExistAsync(string email)
        {
            return _dbContext.Users.AsNoTracking().AnyAsync(x => x.Email == new Email(email));
        }

        //public Task<bool> IsEmergencyContactEmailExistAsync(Guid userId, string email)
        //{
        //    return _dbContext.Users.AsNoTracking().Where(u => u.Id == userId).SelectMany(u => u.EmergencyContacts).AnyAsync(c => c.Email == new Email(email));
        //}

        //public Task<bool> IsEmergencyContactPhoneNumberExistAsync(Guid userId, string phoneNumber)
        //{
        //    return _dbContext.Users.AsNoTracking().Where(u => u.Id == userId).SelectMany(u => u.EmergencyContacts).AnyAsync(c => c.PhoneNumber == new PhoneNumber(phoneNumber));
        //}
    }
}
