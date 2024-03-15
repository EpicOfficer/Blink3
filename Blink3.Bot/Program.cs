using Blink3.Bot.Services;
using Blink3.Common.Caching.Extensions;
using Blink3.Common.Configuration;
using Blink3.Common.Configuration.Extensions;
using Blink3.DataAccess.Extensions;
using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    
    builder.Services.AddAppConfiguration(builder.Configuration);
    BlinkConfiguration appConfig = builder.Configuration.GetAppConfiguration();
    
    builder.Services.AddDataAccess(appConfig);
    builder.Services.AddCaching(appConfig);

    builder.Services.AddDiscordHost((config, _) =>
    {
        config.SocketConfig = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
            AlwaysDownloadUsers = true,
            MessageCacheSize = 200,
            GatewayIntents = GatewayIntents.All
        };
    
        BlinkConfiguration appConfig =
            builder.Configuration.Get<BlinkConfiguration>() ?? throw new InvalidOperationException();
        config.Token = appConfig.Discord.BotToken;
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