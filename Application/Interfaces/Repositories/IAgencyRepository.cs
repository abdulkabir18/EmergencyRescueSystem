using Application.Common.Dtos;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
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
        Task<PaginatedResult<Agency>> GetAllAgenciesAsync(int pageNumber, int pageSize);
        Task<PaginatedResult<Agency>> SearchAgenciesAsync(string keyword, int pageNumber, int pageSize);
        Task<IEnumerable<Agency>> GetAgenciesBySupportedIncidentAsync(IncidentType incidentType);
    }
}
