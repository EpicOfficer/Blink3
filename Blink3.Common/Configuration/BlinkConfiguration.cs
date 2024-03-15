namespace Blink3.Common.Configuration;

public class BlinkConfiguration
{
    public DiscordConfig Discord { get; set; } = null!;
    public ConnectionStringsConfig ConnectionStrings { get; set; } = null!;
    public RedisConfig? Redis { get; set; }
}

public class DiscordConfig
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string BotToken { get; set; } = null!;
}

public class ConnectionStringsConfig
{
    public string DefaultConnection { get; set; } = null!;
}

public class RedisConfig
{
    public string? ConnectionString { get; set; }
}