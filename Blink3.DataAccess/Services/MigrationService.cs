using System.Threading;
using System.Threading.Tasks;
using Blink3.Core.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blink3.DataAccess.Services;

/// <summary>
///     MigrationService is a class responsible for running database migrations.
/// </summary>
public class MigrationService(
    IServiceScopeFactory scopeFactory,
    IOptions<BlinkConfiguration> config,
    ILogger<MigrationService> logger) : IHostedService
{
    /// <summary>
    ///     Represents the configuration options for the Blink application.
    /// </summary>
    private BlinkConfiguration Config => config.Value;

    /// <summary>
    ///     Starts the asynchronous execution of the service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (Config.RunMigrations)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            BlinkDbContext blinkDbContext = GetDbContextFromScope(scope);

            logger.LogInformation("Running database migrations...");
            await blinkDbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Stops the asynchronous operation of the MigrationService.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to stop the operation.</param>
    /// <returns>A task representing the completion of the operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Retrieves the instance of BlinkDbContext from the given IServiceScope.
    /// </summary>
    /// <param name="scope">The IServiceScope from which to retrieve the instance of BlinkDbContext.</param>
    /// <returns>The instance of BlinkDbContext.</returns>
    private static BlinkDbContext GetDbContextFromScope(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<BlinkDbContext>();
    }
}