// ReSharper disable UnusedMember.Global

using System.Linq.Expressions;
using Blink3.Core.Interfaces;

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Represents a generic repository for accessing and manipulating data.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IGenericRepository<T> where T : class, new()
{
    /// <summary>
    ///     Retrieves an object of type T by its id.
    /// </summary>
    /// <param name="keyValues">The id(s) of the object to retrieve.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the retrieved object, or null if
    ///     not found.
    /// </returns>
    Task<T?> GetByIdAsync(params object[] keyValues);

    /// <summary>
    ///     Retrieves an entity by its identifier or creates a new instance if it does not exist.
    /// </summary>
    /// <param name="keyValues">The values representing the identifier of the entity.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an instance of the entity.
    ///     If an instance with the specified identifier is found, it is returned; otherwise, a new instance of the entity is
    ///     created.
    /// </returns>
    Task<T> GetOrCreateByIdAsync(params object[] keyValues);

    /// <summary>
    ///     Retrieves all entities of type T from the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only collection of entities
    ///     of type T.
    /// </returns>
    Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the content of the specified entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated entity.</returns>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);


    /// <summary>
    ///     Updates the properties of an entity asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity to update.</param>
    /// <param name="updatedProperties">The actions that update the properties of the entity.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<T> UpdatePropertiesAsync(T entity, params Action<T>[] updatedProperties);

    /// <summary>
    ///     Deletes an entity asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a record from the repository using the specified ID values.
    /// </summary>
    /// <param name="keyValues">The ID values that uniquely identify the record to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteByIdAsync(params object[] keyValues);
}