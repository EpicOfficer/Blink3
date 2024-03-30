// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;

namespace Blink3.Common.Configuration;

/// <summary>
///     Represents the configuration options for the Blink application.
/// </summary>
public record BlinkConfiguration
{
    /// <summary>
    ///     Represents the Discord configuration.
    /// </summary>
    [Required]
    public DiscordConfig Discord { get; init; } = null!;

    /// <summary>
    ///     Represents the connection strings configuration for the Blink application.
    /// </summary>
    [Required]
    public ConnectionStringsConfig ConnectionStrings { get; init; } = null!;

    /// <summary>
    ///     Represents the Redis configuration.
    /// </summary>
    public RedisConfig? Redis { get; init; }

    /// <summary>
    ///     Represents the allowed origins for the API.
    /// </summary>
    public List<string> ApiAllowedOrigins { get; set; } = [];

    /// <summary>
    ///     Whether to apply pending EF Migrations to the database on startup
    /// </summary>
    public bool RunMigrations { get; set; } = false;
}

/// <summary>
///     Represents the configuration for the Discord component of the Blink application.
/// </summary>
public record DiscordConfig
{
    /// <summary>
    ///     Gets or sets the client ID used for Discord authentication.
    /// </summary>
    [Required]
    public string ClientId { get; init; } = null!;

    /// *Description:**
    [Required]
    public string ClientSecret { get; init; } = null!;

    /// <summary>
    ///     Represents the bot token used to authenticate the Discord bot.
    /// </summary>
    [Required]
    public string BotToken { get; init; } = null!;

    /// <summary>
    ///     The identifier of the development guild.
    /// </summary>
    /// <remarks>
    ///     This property is used to store the unique identifier of the development guild in Discord.
    ///     It is a nullable unsigned 64-bit integer (ulong?) and can be obtained from the <see cref="BlinkConfiguration" />
    ///     instance
    ///     using the <see cref="DiscordConfig.DevGuildId" /> property.
    ///     The development guild is the guild where the bot's commands will be registered.
    ///     If the dev guild id is set, the bot will register its commands to that specific guild.
    ///     If the dev guild id is not set, the bot will register its commands globally.
    ///     This property is used in the <see cref="InteractionHandler" /> class to determine whether to register commands to
    ///     the
    ///     development guild or globally, and also to log the guild id during the registration process.
    /// </remarks>
    public ulong? DevGuildId { get; init; }
}

/// <summary>
///     Represents the configuration for connection strings.
/// </summary>
public record ConnectionStringsConfig
{
    /// <summary>
    ///     Represents the default connection string used in the Blink3 application.
    /// </summary>
    [Required]
    public string DefaultConnection { get; init; } = null!;
}

/// <summary>
///     Represents the configuration settings for Redis.
/// </summary>
public record RedisConfig
{
    /// <summary>
    ///     Represents a configuration for the connection strings.
    /// </summary>
    public string? ConnectionString { get; init; }
}