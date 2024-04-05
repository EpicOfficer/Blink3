using System.Threading.Tasks;
using Blink3.Core.Entities;

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Represents a repository for accessing and manipulating Wordle game data.
/// </summary>
public interface IWordleRepository : IGenericRepository<Wordle>
{
    /// <summary>
    ///     Checks if an entity with the specified ID exists in the repository asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>
    ///     A task representing the asynchronous operation.
    ///     The task result is a boolean indicating whether the entity exists or not.
    /// </returns>
    public Task<bool> ExistsByIdAsync(int id);
}