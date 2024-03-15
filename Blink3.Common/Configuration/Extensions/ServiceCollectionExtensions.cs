using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        return services.AddSingleton(configuration.GetAppConfiguration());
    }
}