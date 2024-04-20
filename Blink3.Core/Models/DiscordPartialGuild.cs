namespace Blink3.Core.Models;

public class DiscordPartialGuild
{
    public ulong Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? IconUrl { get; set; }
}