using Blink3.Bot.Services;
using Blink3.Common.DIExtensions;
using Blink3.DataAccess.DIExtensions;
using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

if (!EF.IsDesignTime)
{
    Log.Logger = new LoggerConfiguration()
       .MinimumLevel.Information()
       .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
       .WriteTo.Console()
       .CreateLogger(); 
}

try
{
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog();
    builder.Services.AddDataAccess(builder.Configuration);
    builder.Services.AddCaching(builder.Configuration);

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
        config.DefaultRunMode = RunMode.Async;
        config.LogLevel = LogSeverity.Info;
        config.UseCompiledLambda = true;
    });

    builder.Services.AddHostedService<InteractionHandler>();
    builder.Services.AddHostedService<BotStatusService>();

    IHost host = builder.Build();

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