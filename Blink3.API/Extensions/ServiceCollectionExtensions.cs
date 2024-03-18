using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OAuth.Discord;
using Blink3.API.Models;
using Blink3.Common.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Serilog;
namespace Blink3.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordAuth(this IServiceCollection services, BlinkConfiguration appConfig)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddOAuth(DiscordAuthenticationDefaults.AuthenticationScheme, options => ConfigureOAuthOptions(options, appConfig));

        return services;
    }

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

    private static async Task FetchDiscordUserInfoAndCreateClaims(OAuthCreatingTicketContext context, OAuthOptions options) 
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, options.UserInformationEndpoint);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        HttpResponseMessage backchannelResponse = await options.Backchannel.SendAsync(requestMessage, context.HttpContext.RequestAborted);

        if (backchannelResponse.IsSuccessStatusCode)
        {
            string userInformationJson = await backchannelResponse.Content.ReadAsStringAsync();
            DiscordUserIdentity userInformation = JsonSerializer.Deserialize<DiscordUserIdentity>(userInformationJson)
                                                      ?? throw new InvalidOperationException("Unable to parse json response from user information endpoint.");

            context.Identity?.AddClaim(new Claim(ClaimTypes.NameIdentifier, userInformation.Id));
            context.Identity?.AddClaim(new Claim(ClaimTypes.Name, userInformation.Username));

            if (userInformation.GlobalName is not null)
            {
                context.Identity?.AddClaim(new Claim(ClaimTypes.GivenName, userInformation.GlobalName));
            }

            if (userInformation.Locale is not null)
            {
                context.Identity?.AddClaim(new Claim(ClaimTypes.Locality, userInformation.Locale));
            }
        }
        else
        {
            Log.Warning("Could not retrieve user data for token {Token}", context.AccessToken);
        }
    }
}