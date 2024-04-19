using System.Net.Http.Json;
using System.Text;
using Blink3.Core.Entities;
using Blink3.Core.Helpers;
using Blink3.Web.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace Blink3.Web.Services;

public class BlinkGuildConfigService(HttpClient httpClient) : IBlinkGuildConfigService
{
    public async Task<BlinkGuild?> GetByIdAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return await httpClient.GetFromJsonAsync<BlinkGuild>($"api/BlinkGuilds/{id}");
    }

    public async Task<bool> PatchAsync(string? id, JsonPatchDocument<BlinkGuild> patchDocument)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        HttpMethod method = new("PATCH");
        
        JsonSerializerSettings options = new();
        options.Converters.Add(new ULongToStringConverter());
        
        HttpRequestMessage request = new(method, $"api/BlinkGuilds/{id}")
        {
            Content = new StringContent(JsonConvert.SerializeObject(patchDocument, options),
                Encoding.UTF8, "application/json-patch+json")
        };

        HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(request);

        return httpResponseMessage.IsSuccessStatusCode;
    }
}