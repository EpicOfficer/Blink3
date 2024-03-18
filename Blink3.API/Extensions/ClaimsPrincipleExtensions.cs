using System.Security.Claims;
using Blink3.API.Models;

namespace Blink3.API.Extensions;

public static class ClaimsPrincipleExtensions
{
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