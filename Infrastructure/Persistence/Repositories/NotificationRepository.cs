using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly ProjectDbContext _dbContext;
        public NotificationRepository(ProjectDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ICollection<Notification> notifications)
        {
            await _dbContext.Notifications.AddRangeAsync(notifications);
        }
    }
}
