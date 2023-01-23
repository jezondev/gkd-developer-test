using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    /// <inheritdoc/>
    public abstract class RepositoryBase<T> : IRepository<T> where T : class
    {
        private readonly DbContext _dbContext;

        protected RepositoryBase(DbContext dbContext)
        {
            this._dbContext = dbContext;
        }
        
        /// <inheritdoc/>
        public virtual async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
        {
            return await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        }
        
        /// <inheritdoc/>
        public virtual async Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<T>().ToListAsync(cancellationToken);
        }
    }
}
