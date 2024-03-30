using Blink3.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blink3.DataAccess.Services;

/// <summary>
/// MigrationService is a class responsible for running database migrations.
/// </summary>
public class MigrationService(IServiceProvider services,
    IOptions<BlinkConfiguration> config,
    ILogger<MigrationService> logger) : IHostedService
{
    /// <summary>
    /// Represents the configuration options for the Blink application.
    /// </summary>
    private BlinkConfiguration Config => config.Value;

    /// <summary>
    /// Starts the asynchronous execution of the service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (Config.RunMigrations)
        {
            IServiceScope scope = services.CreateScope();
            BlinkDbContext blinkDbContext = scope.ServiceProvider.GetRequiredService<BlinkDbContext>();
            
            logger.LogInformation("Running database migrations...");
            await blinkDbContext.Database.MigrateAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Stops the asynchronous operation of the MigrationService.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to stop the operation.</param>
    /// <returns>A task representing the completion of the operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}