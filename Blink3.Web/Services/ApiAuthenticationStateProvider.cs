using System.Net.Http.Json;
using System.Security.Claims;
using Blink3.Core.DiscordAuth;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blink3.Web.Services;

/// <summary>
///     Provides the authentication state for the API.
/// </summary>
public class ApiAuthenticationStateProvider(HttpClient httpClient) : AuthenticationStateProvider
{
    private const string LoginStateKey = "Blink3_LoginState";
    private const string LoginAuthType = "ApiAuth";
    private const string BasePath = "api/auth";

    /// <summary>
    ///     Creates a ClaimsPrincipal object based on the provided AuthStatus object.
    /// </summary>
    /// <param name="status">The authentication status of a user</param>
    /// <returns>The newly created ClaimsPrincipal object</returns>
    private static ClaimsPrincipal CreateClaimsPrincipal(AuthStatus status)
    {
        IEnumerable<Claim> claims = status.ToClaims();
        ClaimsIdentity identity = new(claims, LoginAuthType);
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    ///     Creates an instance of <see cref="AuthenticationState" /> based on the given authentication status.
    /// </summary>
    /// <param name="status">The authentication status of the user.</param>
    /// <returns>An <see cref="AuthenticationState" /> object representing the current authentication state of the user.</returns>
    private static AuthenticationState CreateAuthenticationState(AuthStatus status)
    {
        return new AuthenticationState(CreateClaimsPrincipal(status));
    }

    /// <inheritdoc />
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        AuthStatus? authStatus = await httpClient.GetFromJsonAsync<AuthStatus>($"{BasePath}/status");

        return authStatus is null or { Authenticated: false }
            ? new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))
            : CreateAuthenticationState(authStatus);
    }

    /// <summary>
    ///     Notifies the authentication state provider that the user has logged out.
    /// </summary>
    public void NotifyLogout()
    {
        ClaimsPrincipal anonymousUser = new(new ClaimsIdentity());
        Task<AuthenticationState> authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }
}