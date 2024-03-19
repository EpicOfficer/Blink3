using Blink3.DataAccess.Entities;

namespace Blink3.Web.Interfaces;

/// <summary>
/// Represents a repository for managing UserTodo entities.
/// </summary>
public interface ITodoRepository
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
}