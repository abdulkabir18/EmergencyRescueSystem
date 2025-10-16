using Application.Interfaces.UnitOfWork;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProjectDbContext _context;

        public UnitOfWork(ProjectDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            int result =  await _context.SaveChangesAsync(cancellationToken);

            await _context.DispatchDomainEventsAsync();

            return result;
        }
    }
}
