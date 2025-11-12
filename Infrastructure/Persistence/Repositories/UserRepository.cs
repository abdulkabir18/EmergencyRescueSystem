using Application.Common.Dtos;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
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

        public async Task<PaginatedResult<User>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Users.AsNoTracking().Where(u => !u.IsDeleted);

            var totalCount = query.Count();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<User>.Create(users, totalCount, pageNumber, pageSize);
        }

        public async Task<PaginatedResult<User>> GetAllUsersByRoleAsync(UserRole role, int pageNumber, int pageSize)
        {
            var query = _dbContext.Users.AsNoTracking().Where(u => !u.IsDeleted && u.Role == role);

            var totalCount = query.Count();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<User>.Create(users, totalCount, pageNumber, pageSize);
        }

        public async Task<User?> GetAsync(Expression<Func<User, bool>> expression)
        {
            return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(expression);
        }

        public async Task<User> GetSuperAdminId()
        {
            return await _dbContext.Users.AsNoTracking().Where(u => u.Role == UserRole.SuperAdmin && !u.IsDeleted).FirstAsync();
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _dbContext.Users.AsNoTracking().CountAsync(u => !u.IsDeleted);
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

        public async Task<bool> IsUserExistByIdAsync(Guid userId)
        {
            return await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId);
        }

        public async Task<PaginatedResult<User>> SearchUsersAsync(string keyword, int pageNumber, int pageSize)
        {
            keyword = keyword?.ToLower() ?? "";

            //var query = _dbContext.Users
            //.Select(u => new
            //{
            //    User = u,
            //    Email = u.Email.Value
            //})
            //.Where(x =>
            //    EF.Functions.ILike(x.FullName, $"%{keyword}%") ||
            //    EF.Functions.ILike(x.Email, $"%{keyword}%"));

            var query = _dbContext.Users
                .Where(u =>
                    EF.Functions.ILike(u.FullName, $"%{keyword}%") ||
                    EF.Functions.ILike(u.Email, $"%{keyword}%"));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<User>.Create(items, totalCount, pageNumber, pageSize);
        }

        //public async Task<PaginatedResult<User>> SearchUsersAsync(string keyword, int pageNumber, int pageSize)
        //{
        //    keyword = keyword?.ToLower() ?? "";

        //    var query = _dbContext.Users
        //        .Where(u =>
        //            EF.Functions.Like(u.FullName.ToLower(), $"%{keyword}%") ||
        //            EF.Functions.Like(u.Email.Value.ToLower(), $"%{keyword}%"));

        //    var totalCount = await query.CountAsync();

        //    var items = await query
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //   return PaginatedResult<User>.Create(items, totalCount, pageNumber, pageSize);
        //}


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
