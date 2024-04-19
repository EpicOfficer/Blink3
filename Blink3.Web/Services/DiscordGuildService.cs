using System.Net.Http.Json;
using Blink3.Core.Models;
using Blink3.Web.Interfaces;

namespace Blink3.Web.Services;

public class DiscordGuildService(HttpClient httpClient) : IDiscordGuildService
{
    public async Task<IEnumerable<DiscordPartialChannel>> GetChannels(string? guildId)
    {
        if (string.IsNullOrWhiteSpace(guildId)) return [];
        return await httpClient.GetFromJsonAsync<IEnumerable<DiscordPartialChannel>>(
            $"/api/Guilds/{guildId}/categories") ?? [];
    }

    public async Task<IEnumerable<DiscordPartialChannel>> GetCategories(string? guildId)
    {
        if (string.IsNullOrWhiteSpace(guildId)) return [];
        return await httpClient.GetFromJsonAsync<IEnumerable<DiscordPartialChannel>>(
            $"/api/Guilds/{guildId}/channels") ?? [];
    }
}