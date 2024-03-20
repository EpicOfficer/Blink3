using System.Net.Http.Json;
using System.Text.Json;
using Blink3.Common.Models;
using Blink3.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blink3.Web.Services;

/// <inheritdoc />
public class AuthenticationService(HttpClient httpClient, NavigationManager navigationManager, AuthenticationStateProvider authenticationStateProvider) : IAuthenticationService
{
    private const string BasePath = "api/auth";
    private string BaseUrl => $"{httpClient.BaseAddress?.ToString()}{BasePath}";

    public void LogIn() =>
        navigationManager.NavigateTo($"{BaseUrl}/login?returnUrl={navigationManager.BaseUri}");

    public async Task LogOutAsync()
    {
        await httpClient.GetAsync($"{BasePath}/logout");
        ((ApiAuthenticationStateProvider)authenticationStateProvider).NotifyLogout();
        navigationManager.NavigateTo("/");
    }
}