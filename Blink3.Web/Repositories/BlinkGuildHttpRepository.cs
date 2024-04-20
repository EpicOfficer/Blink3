using System.Net.Http.Json;
using Blink3.Core.Entities;
using Blink3.Web.Interfaces;

namespace Blink3.Web.Repositories;

/// <inheritdoc />
public class BlinkGuildHttpRepository(HttpClient httpClient) : IBlinkGuildHttpRepository
{
    private const string BasePath = "api/blinkguilds";

    public async Task<IEnumerable<BlinkGuild>> GetAsync()
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<BlinkGuild>>($"{BasePath}") ?? [];
    }

    public async Task<BlinkGuild> GetAsync(ulong id)
    {
        return await httpClient.GetFromJsonAsync<BlinkGuild>($"{BasePath}/{id}") ?? new BlinkGuild
        {
            Id = id
        };
    }

    public async Task UpdateAsync(ulong id, BlinkGuild blinkGuild)
    {
        await httpClient.PutAsJsonAsync($"{BasePath}/{id}", blinkGuild);
    }
}