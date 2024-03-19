// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.Security.Claims;

namespace Blink3.Common.Models;

public class AuthStatus
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? GlobalName { get; set; }
    public string? Locale { get; set; }
    public bool Authenticated { get; set; }

    public IEnumerable<Claim> ToClaims()
    {
        List<Claim> claims = [];

        if (!Authenticated) return claims;
        
        claims.Add(new Claim(ClaimTypes.NameIdentifier,
            Id ?? throw new InvalidOperationException()));
        claims.Add(new Claim(ClaimTypes.Name,
            Username ?? throw new InvalidOperationException()));
            
        if (!string.IsNullOrWhiteSpace(GlobalName))
            claims.Add(new Claim(ClaimTypes.GivenName, GlobalName));
            
        if (!string.IsNullOrWhiteSpace(Locale))
            claims.Add(new Claim(ClaimTypes.Locality, Locale));

        return claims;
    }
}