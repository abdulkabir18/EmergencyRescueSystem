using Application.Common.Interfaces.Repositories;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task AddAsync(ICollection<Notification> notifications);
    }
}
