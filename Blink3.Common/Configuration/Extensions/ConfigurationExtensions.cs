using Microsoft.Extensions.Configuration;

namespace Blink3.Common.Configuration.Extensions;

/// <summary>
/// Provides extension methods for IConfiguration objects.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Retrieves the application configuration from the provided <see cref="IConfiguration"/> object.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/> object.</param>
    /// <returns>The <see cref="BlinkConfiguration"/> object containing the application configuration.</returns>
    public static BlinkConfiguration GetAppConfiguration(this IConfiguration configuration)
    {
        return configuration.Get<BlinkConfiguration>() ?? throw new InvalidOperationException("Invalid app configuration");
    }
}