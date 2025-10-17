using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories
{
    public interface IAgencyRepository : IGenericRepository<Agency>
    {
        Task<Agency?> GetAsync(Expression<Func<Agency, bool>> expression);
        Task<bool> IsAgencyExist(Guid AgencyId);
        Task<bool> IsNameExistAsync(string name);
        Task<bool> IsEmailExistAsync(string email);
        Task<bool> IsPhoneNumberExistAsync(string phoneNumber);
    }
}
