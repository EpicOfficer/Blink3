using System.Threading.Tasks;

namespace Blink3.Web.Interfaces;

/// <summary>
///     Represents an authentication service that handles user login, logout, and authentication status.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    ///     Attempts to log in the user.
    /// </summary>
    public void LogIn();

    /// <summary>
    ///     Logs out the current authenticated user.
    /// </summary>
    /// <returns>A task representing the asynchronous logout operation.</returns>
    public Task LogOutAsync();
}