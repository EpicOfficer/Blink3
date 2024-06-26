using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Blink3.Core.Configuration.Extensions;

/// <summary>
///     Extension methods for IServiceCollection to add application configuration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Extension method to add application configuration to the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the configuration to.</param>
    /// <param name="configuration">The <see cref="IConfiguration" /> instance.</param>
    public static void AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BlinkConfiguration>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    /// <summary>
    ///     Retrieves the <see cref="BlinkConfiguration" /> from the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> containing the application configuration.</param>
    /// <returns>The <see cref="BlinkConfiguration" /> object.</returns>
    public static BlinkConfiguration GetAppConfiguration(this IServiceCollection services)
    {
        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<BlinkConfiguration>>().Value;
    }
}