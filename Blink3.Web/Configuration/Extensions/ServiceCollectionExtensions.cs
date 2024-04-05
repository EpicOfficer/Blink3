using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Blink3.Web.Configuration.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="IServiceCollection" /> interface.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the application configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the configuration to.</param>
    /// <param name="configuration">The configuration object.</param>
    public static void AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AppOptions>()
            .Bind(configuration);
    }

    /// <summary>
    ///     Retrieves the application configuration options.
    /// </summary>
    /// <param name="services">The IServiceCollection object.</param>
    /// <returns>The AppOptions object representing the application configuration options.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when the application configuration options are not found.</exception>
    public static AppOptions GetAppConfiguration(this IServiceCollection services)
    {
        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<AppOptions>>().Value;
    }
}