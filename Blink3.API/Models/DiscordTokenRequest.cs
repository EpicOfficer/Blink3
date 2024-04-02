using Newtonsoft.Json;

namespace Blink3.API.Models;

public class DiscordTokenRequest
{
    public string Code { get; set; } = string.Empty;
}