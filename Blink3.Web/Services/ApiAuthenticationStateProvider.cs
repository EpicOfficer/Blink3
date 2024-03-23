using System.Net.Http.Json;
using System.Security.Claims;
using Blink3.Common.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blink3.Web.Services;

public class ApiAuthenticationStateProvider(HttpClient httpClient) : AuthenticationStateProvider
{
    private const string LoginStateKey = "Blink3_LoginState";
    private const string LoginAuthType = "ApiAuth";
    private const string BasePath = "api/auth";

    private static ClaimsPrincipal CreateClaimsPrincipal(AuthStatus status)
    {
        IEnumerable<Claim> claims = status.ToClaims();
        ClaimsIdentity identity = new(claims, LoginAuthType);
        return new ClaimsPrincipal(identity);
    }

    private static AuthenticationState CreateAuthenticationState(AuthStatus status)
    {
        return new AuthenticationState(CreateClaimsPrincipal(status));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        AuthStatus? authStatus = await httpClient.GetFromJsonAsync<AuthStatus>($"{BasePath}/status");

        return authStatus is null or { Authenticated: false }
            ? new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))
            : CreateAuthenticationState(authStatus);
    }

    public void NotifyLogout()
    {
        ClaimsPrincipal anonymousUser = new(new ClaimsIdentity());
        Task<AuthenticationState> authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }
}