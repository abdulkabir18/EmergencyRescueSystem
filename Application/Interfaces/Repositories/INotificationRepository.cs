using Application.Common.Dtos;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task AddAsync(ICollection<Notification> notifications);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<PaginatedResult<Notification>> GetUserNotificationsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
    }
}
