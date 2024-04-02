using System.Security.Claims;

namespace Blink3.Core.DiscordAuth.Extensions;

/// <summary>
///     Contains extension methods for ClaimsPrincipal class.
/// </summary>
public static class ClaimsPrincipleExtensions
{
    /// <summary>
    ///     Retrieves the user ID of the logged-in user.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal object representing the logged-in user.</param>
    /// <returns>The user ID as an unsigned 64-bit integer.</returns>
    public static ulong GetUserId(this ClaimsPrincipal user)
    {
        return ulong.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out ulong userId)
            ? userId
            : throw new InvalidOperationException("Unable to get user ID of logged in user");
    }

    /// <summary>
    ///     Gets the display name of the user.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal object representing the user.</param>
    /// <returns>The display name of the user. If no display name is found, returns an empty string.</returns>
    public static string GetDisplayName(this ClaimsPrincipal user)
    {
        return user.FindFirst(c => c.Type == ClaimTypes.GivenName)?.Value ??
               user.FindFirst(c => c.Type == ClaimTypes.Name)?.Value ??
               string.Empty;
    }

    /// <summary>
    ///     Retrieves the authentication status of the user.
    /// </summary>
    /// <param name="user">The <see cref="ClaimsPrincipal" /> representing the user.</param>
    /// <returns>The authentication status of the user as an instance of <see cref="AuthStatus" />.</returns>
    public static AuthStatus GetAuthStatusModel(this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated is not true)
            return new AuthStatus
            {
                Authenticated = false
            };

        return new AuthStatus
        {
            Id = user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            Username = user.Claims.First(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty,
            GlobalName = user.Claims.First(c => c.Type == ClaimTypes.GivenName)?.Value,
            Locale = user.Claims.First(c => c.Type == ClaimTypes.Locality)?.Value,
            Authenticated = user.Identity?.IsAuthenticated ?? false
        };
    }
}