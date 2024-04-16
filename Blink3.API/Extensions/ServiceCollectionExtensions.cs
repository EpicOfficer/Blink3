using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OAuth.Discord;
using Blink3.Core.Caching;
using Blink3.Core.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Blink3.API.Extensions;

/// <summary>
///     Contains extension methods for the <see cref="IServiceCollection" /> interface to configure authentication with
///     Discord.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds Discord authentication to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="appConfig">The application configuration.</param>
    public static void AddDiscordAuth(this IServiceCollection services, BlinkConfiguration appConfig)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddDiscord(options =>
            {
                options.ClientId = appConfig.Discord.ClientId;
                options.ClientSecret = appConfig.Discord.ClientSecret;
                options.CallbackPath = new PathString("/api/auth/callback");
                
                options.Scope.Add("guilds");

                options.Events.OnCreatingTicket = async context =>
                {
                    // Add your extra claim for GlobalName here
                    if (context.User.GetProperty("global_name") is { ValueKind: JsonValueKind.String } globalNameElement &&
                        globalNameElement.GetString() is { } globalName &&
                        !string.IsNullOrEmpty(globalName))
                    {
                        context.Identity?.AddClaim(new Claim(ClaimTypes.GivenName, globalName));
                    }
                    
                    await SaveTokenAsync(context);
                };
            });
    }

    /// <summary>
    ///     Saves the access token in the caching service.
    /// </summary>
    /// <param name="context">The OAuthCreatingTicketContext.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private static async Task SaveTokenAsync(OAuthCreatingTicketContext context)
    {
        ICachingService cachingService = context.HttpContext.RequestServices.GetRequiredService<ICachingService>();
        string? nameIdentifierClaim = context.Identity?.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (nameIdentifierClaim is not null && context.AccessToken is not null) 
        {
            await cachingService.SetAsync($"token:{nameIdentifierClaim}", context.AccessToken, context.ExpiresIn);
        }
    }
}