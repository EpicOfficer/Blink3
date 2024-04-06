using System.Text.Json.Serialization;

namespace Blink3.API.Models;

public class DiscordTokenResponse
{
    [JsonPropertyName("token_type")] public string? TokenType { get; set; }

    [JsonPropertyName("access_token")] public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; }

    [JsonPropertyName("scope")] public string? Scope { get; set; }
}