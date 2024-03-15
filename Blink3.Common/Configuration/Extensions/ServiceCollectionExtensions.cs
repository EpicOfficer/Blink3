using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blink3.Common.Configuration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        BlinkConfiguration appConfig = new()
        {
            Discord = configuration.GetSection(nameof(BlinkConfiguration.DiscordConfig)).Get<BlinkConfiguration.DiscordConfig>() ?? throw new InvalidOperationException(),
            ConnectionStrings = configuration.GetSection(nameof(BlinkConfiguration.ConnectionStringsConfig)).Get<BlinkConfiguration.ConnectionStringsConfig>() ?? throw new InvalidOperationException(),
            Redis = configuration.GetSection(nameof(BlinkConfiguration.RedisConfig)).Exists() ?
                configuration.GetSection(nameof(BlinkConfiguration.RedisConfig)).Get<BlinkConfiguration.RedisConfig>() : null
        };

        return services.AddSingleton(appConfig);
    }
}