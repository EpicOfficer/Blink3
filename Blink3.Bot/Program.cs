using Blink3.Bot.Services;
using Blink3.DataAccess.Interfaces;
using Blink3.DataAccess;
using Blink3.DataAccess.Repositories;
using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .WriteTo.Console()
    .CreateLogger();

var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration.AddConfiguration(configurationBuilder.Build());

    builder.Services.AddSerilog();

    builder.Services.AddDbContext<BlinkDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
    
    builder.Services.AddDiscordHost((config, _) =>
    {
        config.SocketConfig = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
            AlwaysDownloadUsers = true,
            MessageCacheSize = 200,
            GatewayIntents = GatewayIntents.All
        };
    
        config.Token = builder.Configuration["Token"]!;
    });

    builder.Services.AddInteractionService((config, _) =>
    {
        config.LogLevel = LogSeverity.Info;
        config.UseCompiledLambda = true;
    });

    builder.Services.AddHostedService<InteractionHandler>();
    builder.Services.AddHostedService<BotStatusService>();

    builder.Services.AddScoped<IBlinkGuildRepository, BlinkGuildRepository>();

    var host = builder.Build();

    await host.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}