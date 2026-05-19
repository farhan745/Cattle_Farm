namespace CattleFarm.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository interface providing standard CRUD operations.
    /// All entity-specific repositories must implement this contract.
    /// </summary>
    /// <typeparam name="T">The entity type managed by this repository.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>Returns all records of type T.</summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>Returns a single record by its primary key, or null if not found.</summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>Adds a new entity to the context (not yet saved).</summary>
        Task AddAsync(T entity);

        /// <summary>Marks an existing entity as modified (not yet saved).</summary>
        void Update(T entity);

        /// <summary>Marks an entity for deletion (not yet saved).</summary>
        void Delete(T entity);

        /// <summary>Returns whether any entity satisfies the given predicate.</summary>
        Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    }
}
