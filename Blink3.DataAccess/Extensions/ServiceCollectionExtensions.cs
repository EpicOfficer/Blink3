using Blink3.Common.Configuration;
using Blink3.DataAccess.Interfaces;
using Blink3.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Blink3.DataAccess.Extensions;

/// <summary>
///     Contains extension methods for configuring data access services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the data access services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, BlinkConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionStrings.DefaultConnection))
            services.AddDbContext<BlinkDbContext>(options =>
                options.UseSqlite(configuration.ConnectionStrings.DefaultConnection));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IBlinkGuildRepository, BlinkGuildRepository>();
        services.AddScoped<IUserTodoRepository, UserTodoRepository>();

        return services;
    }
}