namespace Core.Interfaces
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T">The type of entity being operated on by this repository.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Finds an entity with the given primary key value.
        /// </summary>
        /// <typeparam name="TId">The type of primary key.</typeparam>
        /// <param name="id">The value of the primary key for the entity to be found.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <typeparamref name="T" />, or <see langword="null"/>.
        /// </returns>
        Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull;
        
        /// <summary>
        /// Finds all entities of <typeparamref name="T" /> from the dataStore.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
        /// </returns>
        Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
    }
}
