using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Blink3.Common.Configuration.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to add application configuration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Extension method to add application configuration to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the configuration to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BlinkConfiguration>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }

    /// <summary>
    /// Retrieves the <see cref="BlinkConfiguration"/> from the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> containing the application configuration.</param>
    /// <returns>The <see cref="BlinkConfiguration"/> object.</returns>
    public static BlinkConfiguration GetBlinkConfiguration(this IServiceCollection services)
    {
        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<BlinkConfiguration>>()?.Value ?? throw new InvalidOperationException();
    }
}