using System.Security.Claims;
using Blazored.LocalStorage;
using Blink3.Common.Models;
using Blink3.Web.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blink3.Web.Services;

public class ApiAuthenticationStateProvider(IAuthenticationService authenticationService) : AuthenticationStateProvider
{
    private const string LoginStateKey = "Blink3_LoginState";
    private const string LoginAuthType = "ApiAuth";

    private static ClaimsPrincipal CreateClaimsPrincipal(AuthStatus status)
    {
        IEnumerable<Claim> claims = status.ToClaims();
        ClaimsIdentity identity = new(claims, LoginAuthType);
        return new ClaimsPrincipal(identity); 
    }

    private static AuthenticationState CreateAuthenticationState(AuthStatus status)
        => new AuthenticationState(CreateClaimsPrincipal(status));
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        AuthStatus? authStatus = await authenticationService.GetStatusAsync();
        return authStatus is null or {Authenticated: false} ?
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())) :
            CreateAuthenticationState(authStatus);
    }
}