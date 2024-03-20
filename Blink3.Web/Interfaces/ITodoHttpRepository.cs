using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Models;

namespace Blink3.Web.Interfaces;

/// <summary>
/// Represents a repository for managing UserTodo entities from the API.
/// </summary>
public interface ITodoHttpRepository
{
    /// <summary>
    /// Retrieves all the UserTodo entities.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns a collection of UserTodo objects.</returns>
    public Task<IReadOnlyCollection<UserTodo>> GetAsync();

    /// <summary>
    /// Retrieves all UserTodo entities asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains a read-only collection of UserTodo entities.</returns>
    public Task<UserTodo> GetAsync(int id);

    /// <summary>
    /// Adds a new UserTodo asynchronously.
    /// </summary>
    /// <param name="todoDto">The UserTodoDto object containing the details of the UserTodo to be added.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the added UserTodo object.</returns>
    public Task<UserTodo> AddAsync(UserTodoDto todoDto);

    /// <summary>
    /// Updates an existing UserTodo entity asynchronously.
    /// </summary>
    /// <param name="id">The ID of the UserTodo entity to update.</param>
    /// <param name="todoDto">The UserTodoDto object containing the updated information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(int id, UserTodoDto todoDto);

    /// <summary>
    /// Deletes a UserTodo entity asynchronously.
    /// </summary>
    /// <param name="id">The ID of the UserTodo entity to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteAsync(int id);
}