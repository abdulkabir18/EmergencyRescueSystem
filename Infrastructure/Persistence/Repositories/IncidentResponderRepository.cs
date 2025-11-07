using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class IncidentResponderRepository : GenericRepository<IncidentResponder>, IIncidentResponderRepository
    {
        private readonly ProjectDbContext _dbContext;
        public IncidentResponderRepository(ProjectDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> CheckRoleAssignedAsync(Guid incidentId, ResponderRole role)
        {
            return await _dbContext.IncidentResponders.AsNoTracking().AnyAsync(r => r.IncidentId == incidentId && r.Role == role && r.IsActive && !r.IsDeleted);
        }

    }
}