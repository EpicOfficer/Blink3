using Blink3.Common.Models;

namespace Blink3.Web.Interfaces;

/// <summary>
/// Represents an authentication service that handles user login, logout, and authentication status.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Attempts to log in the user asynchronously.
    /// </summary>
    /// <param name="returnUrl">The URL to return to after successful login.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result will be an instance
    /// of <see cref="AuthStatus"/> that contains the authentication status.
    /// </returns>
    public Task LoginAsync();

    /// <summary>
    /// Logs out the current authenticated user.
    /// </summary>
    /// <returns>A task representing the asynchronous logout operation.</returns>
    public Task LogoutAsync();

    /// <summary>
    /// Asynchronously gets the authentication status.
    /// </summary>
    /// <returns>The authentication status as an instance of <see cref="AuthStatus"/>.</returns>
    public Task<AuthStatus?> GetStatusAsync();
}