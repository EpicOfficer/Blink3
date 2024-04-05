// ReSharper disable UnusedMember.Global

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Represents a generic repository for accessing and manipulating data.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    ///     Retrieves an entity by its key values asynchronously.
    /// </summary>
    /// <param name="keyValues">The key values of the entity.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the entity if found, otherwise null.</returns>
    Task<T?> GetByIdAsync(params object[] keyValues);

    /// <summary>
    ///     Gets all entities of type T asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities of type T.</returns>
    Task<IReadOnlyCollection<T>> GetAllAsync();

    /// <summary>
    ///     Adds a new entity to the database asynchronously.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the added entity.</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    ///     Updates the specified entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    ///     Deletes the specified entity from the database.
    /// </summary>
    /// <param name="entity">The entity to be deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(T entity);

    /// <summary>
    ///     Deletes an entity from the database by it's key values asynchronously.
    /// </summary>
    /// <param name="keyValues">The key values of the entity.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteByIdAsync(params object[] keyValues);
}