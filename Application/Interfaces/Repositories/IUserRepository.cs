using Application.Common.Dtos;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetSuperAdminId();
        Task<User?> GetAsync(Expression<Func<User, bool>> expression);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> IsEmailExistAsync(string email);
        Task<bool> IsUserExistByIdAsync(Guid userId);
        Task<PaginatedResult<User>> GetAllUsersByRoleAsync(UserRole role, int pageNumber, int pageSize);
        Task<PaginatedResult<User>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<PaginatedResult<User>> SearchUsersAsync(string keyword,int pageNumber, int pageSize);
        Task<int> GetTotalUsersCountAsync();
        //Task<IEnumerable<string>> GetEmergencyContactEmailsAsync(Guid userId);
        //Task<bool> IsEmergencyContactEmailExistAsync(Guid userId, string email);
        //Task<bool> IsEmergencyContactPhoneNumberExistAsync(Guid userId, string phoneNumber);
    }
}
