// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;

namespace Blink3.Common.Configuration;

public record BlinkConfiguration
{
    [Required]
    public DiscordConfig Discord { get; init; } = null!;
    
    [Required]
    public ConnectionStringsConfig ConnectionStrings { get; init; } = null!;
    
    public RedisConfig? Redis { get; init; }
}

public record DiscordConfig
{
    [Required]
    public string ClientId { get; init; } = null!;
    
    [Required]
    public string ClientSecret { get; init; } = null!;
    
    [Required]
    public string BotToken { get; init; } = null!;
}

public record ConnectionStringsConfig
{
    [Required]
    public string DefaultConnection { get; init; } = null!;
}

public record RedisConfig
{
    public string? ConnectionString { get; init; }
}