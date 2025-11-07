using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories
{
    public interface IIncidentResponderRepository : IGenericRepository<IncidentResponder>
    {
        Task<bool> CheckRoleAssignedAsync(Guid incidentId, ResponderRole role);
    }
}
