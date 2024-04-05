using System.Threading.Tasks;
using Blink3.API.Models;

namespace Blink3.API.Interfaces;

/// <summary>
///     Represents the interface for handling Discord token operations.
/// </summary>
public interface IDiscordTokenService
{
    /// <summary>
    ///     Retrieves a Discord access token asynchronously.
    /// </summary>
    /// <param name="code">The authorization code received from Discord.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a
    ///     <see cref="DiscordTokenResponse" /> object representing the Discord access token information.
    /// </returns>
    Task<DiscordTokenResponse> GetTokenAsync(string code);
}