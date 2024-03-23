// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.Security.Claims;

namespace Blink3.Common.Models;

/// <summary>
///     Represents the authentication status of a user.
/// </summary>
public class AuthStatus
{
    public string? Id { get; set; }

    public string? Username { get; set; }

    public string? GlobalName { get; set; }

    public string? Locale { get; set; }

    /// <summary>
    ///     Gets or sets the authentication status of the user.
    /// </summary>
    /// <remarks>
    ///     This property indicates whether the user is authenticated or not.
    /// </remarks>
    /// <value>
    ///     <c>true</c> if the user is authenticated; otherwise, <c>false</c>.
    /// </value>
    public bool Authenticated { get; set; }

    /// <summary>
    ///     Converts the <see cref="AuthStatus" /> object to a collection of <see cref="Claim" /> objects.
    /// </summary>
    /// <returns>A collection of <see cref="Claim" /> objects.</returns>
    /// <remarks>
    ///     This method converts the properties of the <see cref="AuthStatus" /> object into <see cref="Claim" /> objects.
    ///     If the <see cref="AuthStatus.Authenticated" /> property is false, an empty collection of claims is returned.
    ///     The following claims are added to the collection:
    ///     - <see cref="ClaimTypes.NameIdentifier" />: The value of the <see cref="AuthStatus.Id" /> property. An
    ///     <see cref="InvalidOperationException" /> is thrown if the value is null.
    ///     - <see cref="ClaimTypes.Name" />: The value of the <see cref="AuthStatus.Username" /> property. An
    ///     <see cref="InvalidOperationException" /> is thrown if the value is null.
    ///     - <see cref="ClaimTypes.GivenName" />: The value of the <see cref="AuthStatus.GlobalName" /> property, if it is not
    ///     null or empty.
    ///     - <see cref="ClaimTypes.Locality" />: The value of the <see cref="AuthStatus.Locale" /> property, if it is not null
    ///     or empty.
    /// </remarks>
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