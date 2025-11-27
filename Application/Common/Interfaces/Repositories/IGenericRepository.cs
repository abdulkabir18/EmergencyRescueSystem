using Domain.Common;
using System.Linq.Expressions;

namespace Application.Common.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : AuditableEntity
    {
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task<T?> GetAsync(Guid id);
        Task<T?> GetForUpdateAsync(Guid id);
        void Attach(T entity);
        Task DeleteAsync(T entity);
        Task<int> CountAsync(Expression<Func<T, bool>> expression);
        Task<int> CountAsync();
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> GetAllAsync();
    }
}