using System.Security.Claims;
using Blink3.Common.Models;

namespace Blink3.Common.Extensions;

public static class ClaimsPrincipleExtensions
{
    public static ulong GetUserId(this ClaimsPrincipal user)
        => ulong.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out ulong userId) ? userId
            : throw new InvalidOperationException("Unable to get user ID of logged in user");

    public static string GetDisplayName(this ClaimsPrincipal user)
        => user.FindFirst(c => c.Type == ClaimTypes.GivenName)?.Value ??
           user.FindFirst(c => c.Type == ClaimTypes.Name)?.Value ??
           string.Empty;
    
    public static AuthStatus GetAuthStatusModel(this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated is not true)
        {
            return new AuthStatus()
            {
                Authenticated = false
            };
        }

        return new AuthStatus()
        {
            Id = user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            Username = user.Claims.First(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty,
            GlobalName = user.Claims.First(c => c.Type == ClaimTypes.GivenName)?.Value,
            Locale = user.Claims.First(c => c.Type == ClaimTypes.Locality)?.Value,
            Authenticated = user.Identity?.IsAuthenticated ?? false
        };
    }
}