using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OAuth.Discord;
using Blink3.API.Models;
using Blink3.Core.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Serilog;

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
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddDiscordAuth(this IServiceCollection services, BlinkConfiguration appConfig)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOAuth(DiscordAuthenticationDefaults.AuthenticationScheme,
                options => ConfigureOAuthOptions(options, appConfig));

        return services;
    }

    /// <summary>
    ///     Configures the OAuth authentication options for Discord.
    /// </summary>
    /// <param name="options">The <see cref="OAuthOptions" /> to configure.</param>
    /// <param name="appConfig">The <see cref="BlinkConfiguration" /> instance that contains the Discord configuration.</param>
    private static void ConfigureOAuthOptions(OAuthOptions options, BlinkConfiguration appConfig)
    {
        options.ClientId = appConfig.Discord.ClientId;
        options.ClientSecret = appConfig.Discord.ClientSecret;
        options.CallbackPath = new PathString("/api/auth/callback");
        options.AuthorizationEndpoint = DiscordAuthenticationDefaults.AuthorizationEndpoint;
        options.TokenEndpoint = DiscordAuthenticationDefaults.TokenEndpoint;
        options.UserInformationEndpoint = DiscordAuthenticationDefaults.UserInformationEndpoint;

        options.Scope.Add("identify");
        options.Scope.Add("guilds");

        options.Events.OnCreatingTicket = async context =>
        {
            await FetchDiscordUserInfoAndCreateClaims(context, options);
        };

        options.SaveTokens = true;
    }

    /// <summary>
    ///     Fetches Discord user information and creates claims for authentication ticket.
    /// </summary>
    /// <param name="context">The OAuthCreatingTicketContext object.</param>
    /// <param name="options">The OAuthOptions object.</param>
    private static async Task FetchDiscordUserInfoAndCreateClaims(OAuthCreatingTicketContext context,
        OAuthOptions options)
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, options.UserInformationEndpoint);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        HttpResponseMessage backchannelResponse =
            await options.Backchannel.SendAsync(requestMessage, context.HttpContext.RequestAborted);

        if (backchannelResponse.IsSuccessStatusCode)
        {
            string userInformationJson = await backchannelResponse.Content.ReadAsStringAsync();
            DiscordUserIdentity userInformation = JsonSerializer.Deserialize<DiscordUserIdentity>(userInformationJson)
                                                  ?? throw new InvalidOperationException(
                                                      "Unable to parse json response from user information endpoint.");

            context.Identity?.AddClaim(new Claim(ClaimTypes.NameIdentifier, userInformation.Id));
            context.Identity?.AddClaim(new Claim(ClaimTypes.Name, userInformation.Username));

            if (userInformation.GlobalName is not null)
                context.Identity?.AddClaim(new Claim(ClaimTypes.GivenName, userInformation.GlobalName));

            if (userInformation.Locale is not null)
                context.Identity?.AddClaim(new Claim(ClaimTypes.Locality, userInformation.Locale));
        }
        else
        {
            Log.Warning("Could not retrieve user data for token {Token}", context.AccessToken);
        }
    }
}