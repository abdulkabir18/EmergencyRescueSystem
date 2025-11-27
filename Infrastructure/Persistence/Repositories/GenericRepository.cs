using Application.Common.Interfaces.Repositories;
using Domain.Common;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : AuditableEntity
    {
        private readonly ProjectDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ProjectDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Attach(T entity)
        {
            _dbSet.Attach(entity);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.CountAsync(expression);
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public Task DeleteAsync(T entity)
        {
            entity.SoftDelete();
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.AsNoTracking().Where(expression).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().Where(e => !e.IsDeleted).ToListAsync();
        }

        public async Task<T?> GetAsync(Guid id)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<T?> GetForUpdateAsync(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        }

        public Task UpdateAsync(T entity)
        {
            entity.MarkUpdated();
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }
    }
}
