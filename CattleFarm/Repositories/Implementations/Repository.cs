using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CattleFarm.Repositories.Implementations
{
    /// <summary>
    /// Generic EF Core repository providing default CRUD operations.
    /// Entity-specific repositories inherit from this class and extend it.
    /// </summary>
    /// <typeparam name="T">The entity type managed by this repository.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly CattleFarmDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(CattleFarmDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        /// <inheritdoc/>
        public virtual async Task<T?> GetByIdAsync(int id)
            => await _dbSet.FindAsync(id);

        /// <inheritdoc/>
        public async Task AddAsync(T entity)
            => await _dbSet.AddAsync(entity);

        /// <inheritdoc/>
        public void Update(T entity)
            => _dbSet.Update(entity);

        /// <inheritdoc/>
        public void Delete(T entity)
            => _dbSet.Remove(entity);

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);
    }
}
