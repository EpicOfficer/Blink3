using System.Net.Http.Headers;
using AspNet.Security.OAuth.Discord;
using Blink3.API.Interfaces;
using Blink3.API.Models;
using Blink3.Core.Configuration;
using Microsoft.Extensions.Options;

namespace Blink3.API.Services;

public class DiscordTokenService(
    IHttpClientFactory httpClientFactory,
    IOptions<BlinkConfiguration> config) : IDiscordTokenService
{
    private BlinkConfiguration Config => config.Value;

    public async Task<DiscordTokenResponse> GetTokenAsync(string code)
    {
        using HttpClient httpClient = httpClientFactory.CreateClient();
        FormUrlEncodedContent requestBody = new(new[]
        {
            new KeyValuePair<string, string>("client_id", Config.Discord.ClientId),
            new KeyValuePair<string, string>("client_secret", Config.Discord.ClientSecret),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code)
        });

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response =
            await httpClient.PostAsync(DiscordAuthenticationDefaults.TokenEndpoint, requestBody);
        if (!response.IsSuccessStatusCode)
            throw new ApplicationException("Error retrieving access token from Discord.");

        return await response.Content.ReadFromJsonAsync<DiscordTokenResponse>() ?? new DiscordTokenResponse();
    }
}