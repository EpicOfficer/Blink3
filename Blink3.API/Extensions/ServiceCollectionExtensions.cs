using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OAuth.Discord;
using Blink3.API.Interfaces;
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
            .AddCookie(options =>
            {
                options.Cookie.Name = "blink3.auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.LoginPath = new PathString("/api/auth/login");
                options.LogoutPath = new PathString("/api/auth/logout");
            })
            .AddDiscord(options =>
            {
                options.ClientId = appConfig.Discord.ClientId;
                options.ClientSecret = appConfig.Discord.ClientSecret;
                options.CallbackPath = new PathString("/api/auth/callback");

                options.Scope.Add("guilds");

                options.Events.OnCreatingTicket = async context =>
                {
                    // Add your extra claim for GlobalName here
                    if (context.User.GetProperty("global_name") is
                            { ValueKind: JsonValueKind.String } globalNameElement &&
                        globalNameElement.GetString() is { } globalName &&
                        !string.IsNullOrEmpty(globalName))
                        context.Identity?.AddClaim(new Claim(ClaimTypes.GivenName, globalName));

                    await SaveTokenAsync(context);
                };
            });
    }

    /// <summary>
    ///     Saves the access token in the caching service.
    /// </summary>
    /// <param name="context">The OAuthCreatingTicketContext.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    private static async Task SaveTokenAsync(OAuthCreatingTicketContext context)
    {
        ICachingService cachingService = context.HttpContext.RequestServices.GetRequiredService<ICachingService>();
        IEncryptionService encryptionService =
            context.HttpContext.RequestServices.GetRequiredService<IEncryptionService>();

        string? nameIdentifierClaim = context.Identity?.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (nameIdentifierClaim is not null && context.AccessToken is not null)
        {
            string encryptedToken = encryptionService.Encrypt(context.AccessToken, out string iv);
            string tokenKey = $"token:{nameIdentifierClaim}";

            // Store both the encrypted token and the IV
            await cachingService.SetAsync(tokenKey, encryptedToken, context.ExpiresIn);
            await cachingService.SetAsync($"{tokenKey}:iv", iv, context.ExpiresIn);
        }
    }
}