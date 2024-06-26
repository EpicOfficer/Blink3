using Blink3.Core.Entities;

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Represents a repository for accessing and manipulating UserTodo entities.
/// </summary>
public interface IUserTodoRepository : IGenericRepository<UserTodo>
{
    /// <summary>
    ///     Retrieves a collection of UserTodos by user ID asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a collection of UserTodos.</returns>
    Task<IReadOnlyCollection<UserTodo>> GetByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the count of UserTodos by user ID asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the count of UserTodos.</returns>
    Task<int> GetCountByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Marks a UserTodo as complete by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the UserTodo to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CompleteByIdAsync(int id, CancellationToken cancellationToken = default);
}