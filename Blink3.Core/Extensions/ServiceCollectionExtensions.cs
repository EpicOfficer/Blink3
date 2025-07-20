using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

namespace Blink3.Core.Extensions;

/// <summary>
/// Provides extension methods for IServiceCollection to configure shared services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Configures Serilog for the application with optional Loki integration.
    /// </summary>
    /// <param name="services">The IServiceCollection instance to add Serilog to.</param>
    /// <param name="configuration">Application configuration to retrieve logging-specific settings.</param>
    /// <param name="appName">The name of the application (e.g., "Blink3.API" or "Blink3.Bot").</param>
    /// <returns>The IServiceCollection instance with configured logging.</returns>
    public static IServiceCollection AddBlinkLogging(
        this IServiceCollection services,
        IConfiguration configuration,
        string appName)
    {
        // Retrieve log levels from configuration (or fallback to defaults)
        LogEventLevel defaultLogLevel = configuration.GetValue("LogLevel:Default", LogEventLevel.Information);
        LogEventLevel microsoftLogLevel = configuration.GetValue("LogLevel:Override:Microsoft", LogEventLevel.Error);

        // Create the logger configuration
        LoggerConfiguration loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(defaultLogLevel)
            .MinimumLevel.Override("Microsoft", microsoftLogLevel)
            .WriteTo.Console();

        // Add Loki logging if the Loki URL is provided
        string? lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL");
        if (!string.IsNullOrWhiteSpace(lokiUrl))
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            string hostname = Environment.MachineName;

            loggerConfig.WriteTo.GrafanaLoki(lokiUrl, [
                new LokiLabel { Key = "job", Value = appName },
                new LokiLabel { Key = "environment", Value = environment },
                new LokiLabel { Key = "hostname", Value = hostname }
            ]);
        }
        else
        {
            Console.WriteLine("WARNING: Loki URL not configured! Skipping Loki integration and using Console logging only...");
        }

        // Initialize Serilog globally
        Log.Logger = loggerConfig.CreateLogger();

        // Add Serilog to the dependency injection container
        services.AddSerilog();

        // Return the IServiceCollection to enable fluent calls
        return services;
    }
}