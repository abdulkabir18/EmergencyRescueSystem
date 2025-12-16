using Application.Common.Dtos;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

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

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _dbContext.Notifications.CountAsync(n => n.RecipientId == userId && !n.IsRead);
        }

        public async Task<PaginatedResult<Notification>> GetUserNotificationsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _dbContext.Notifications.AsNoTracking().Where(n => n.RecipientId == userId && !n.IsDeleted).OrderByDescending(n => n.CreatedAt);

            var totalCount = query.Count();

            var notifications = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<Notification>.Success(notifications, totalCount, pageNumber, pageSize);
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            var toUpdate = await _dbContext.Notifications
                .Where(n => n.RecipientId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            if (toUpdate == null || toUpdate.Count == 0)
                return 0;

            foreach (var n in toUpdate)
            {
                n.MarkAsRead();
            }

            _dbContext.Notifications.UpdateRange(toUpdate);
            return toUpdate.Count;
        }
    }
}
